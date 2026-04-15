using GoingMy.Shared.Events;
using GoingMy.User.Domain.Outbox;
using GoingMy.User.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GoingMy.User.Infrastructure.Workers;

/// <summary>
/// Background worker that polls the <see cref="OutboxMessage"/> table and
/// publishes pending messages to Kafka via MassTransit topic producers.
/// Implements the Outbox pattern: messages are written atomically with domain
/// data and delivered asynchronously, guaranteeing no message loss on crashes.
/// </summary>
public class OutboxPublisherWorker(
    IServiceProvider serviceProvider,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetries = 5;
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox publisher worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error in outbox publisher loop.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }

        logger.LogInformation("Outbox publisher worker stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        var pending = await context.OutboxMessages
            .Where(m => m.PublishedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        logger.LogDebug("Processing {Count} pending outbox messages.", pending.Count);

        foreach (var message in pending)
        {
            try
            {
                await PublishAsync(scope.ServiceProvider, message, ct);
                message.PublishedAt = DateTime.UtcNow;
                message.Error = null;

                logger.LogDebug("Published outbox message {Id} ({EventType}).", message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                logger.LogWarning(ex,
                    "Failed to publish outbox message {Id} ({EventType}). Retry {Retry}/{Max}.",
                    message.Id, message.EventType, message.RetryCount, MaxRetries);
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private static Task PublishAsync(IServiceProvider sp, OutboxMessage message, CancellationToken ct) =>
        message.EventType switch
        {
            nameof(UserCreatedEvent) => ProduceAsync<UserCreatedEvent>(sp, message, ct),
            nameof(UserUpdatedEvent) => ProduceAsync<UserUpdatedEvent>(sp, message, ct),
            nameof(UserDeletedEvent) => ProduceAsync<UserDeletedEvent>(sp, message, ct),
            _ => Task.CompletedTask
        };

    private static async Task ProduceAsync<T>(IServiceProvider sp, OutboxMessage message, CancellationToken ct)
        where T : class
    {
        var producer = sp.GetRequiredService<ITopicProducer<T>>();
        var payload = JsonSerializer.Deserialize<T>(message.Payload)
            ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from outbox message {message.Id}.");

        await producer.Produce(payload, ct);
    }
}
