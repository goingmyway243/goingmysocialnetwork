using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Commands;
using GoingMy.Notification.Application.Consumers;
using GoingMy.Notification.Application.Dtos;
using GoingMy.Notification.Application.Queries;
using GoingMy.Notification.Domain.Enums;
using GoingMy.Shared.Events;
using GoingMy.Upload.Application.Saga;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GoingMy.Notification.Tests;

public class PostWithMediaSagaFlowIntegrationTests
{
    [Fact]
    public async Task Should_publish_completion_from_saga_and_consume_in_notification_consumer()
    {
        var mediator = new FlowMediator();
        var pushService = new FlowPushService();

        await using var provider = new ServiceCollection()
            .AddSingleton<IMediator>(mediator)
            .AddSingleton<INotificationPushService>(pushService)
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<PostMediaStateMachine, PostMediaSagaState>()
                    .InMemoryRepository();
                x.AddConsumer<PostWithMediaSagaCompletedNotificationConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var correlationId = Guid.NewGuid();
            var fileIds = new[] { "file-1" };

            await harness.Bus.Publish(new PostWithMediaRequestedEvent
            {
                CorrelationId = correlationId,
                UserId = "user-integration",
                Username = "integration-user",
                Content = "integration content",
                MediaFileIds = fileIds
            });

            await harness.Bus.Publish(new MediaValidatedEvent
            {
                CorrelationId = correlationId,
                MediaFileIds = fileIds,
                MediaFiles = [new MediaFileInfo("file-1", "https://cdn/file-1.jpg", "image/jpeg")]
            });

            await harness.Bus.Publish(new PostCreatedForSagaEvent
            {
                CorrelationId = correlationId,
                PostId = "post-integration"
            });

            await harness.Bus.Publish(new MediaAttachedToPostEvent
            {
                CorrelationId = correlationId,
                PostId = "post-integration"
            });

            Assert.True(await harness.Published.Any<PostWithMediaSagaCompletedEvent>(x =>
                x.Context.Message.CorrelationId == correlationId &&
                x.Context.Message.IsSuccess &&
                x.Context.Message.PostId == "post-integration"));

            Assert.True(await harness.Consumed.Any<PostWithMediaSagaCompletedEvent>(x =>
                x.Context.Message.CorrelationId == correlationId &&
                x.Context.Message.IsSuccess));

            Assert.Contains(mediator.Requests, r =>
                r is CreateNotificationCommand cmd &&
                cmd.RecipientUserId == "user-integration" &&
                cmd.Type == NotificationType.PostWithMediaCreated &&
                cmd.ReferenceId == "post-integration");

            Assert.Contains(mediator.Requests, r =>
                r is GetUnreadCountQuery q &&
                q.UserId == "user-integration");

            Assert.Single(pushService.PushedNotifications);
            Assert.Equal(NotificationType.PostWithMediaCreated, pushService.PushedNotifications[0].Type);
            Assert.Equal("user-integration", pushService.PushedNotifications[0].RecipientUserId);
            Assert.Single(pushService.UnreadCountPushes);
            Assert.Equal("user-integration", pushService.UnreadCountPushes[0].UserId);
        }
        finally
        {
            await harness.Stop();
        }
    }

    private sealed class FlowMediator : IMediator
    {
        public List<object> Requests { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;

        public Task Publish<TNotification>(object notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            Requests.Add(request!);
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            return request switch
            {
                CreateNotificationCommand cmd => Task.FromResult<object?>(
                    new NotificationDto(
                        Id: "notif-flow",
                        RecipientUserId: cmd.RecipientUserId,
                        ActorUserId: cmd.ActorUserId,
                        ActorUsername: cmd.ActorUsername,
                        ActorAvatarUrl: cmd.ActorAvatarUrl,
                        Type: cmd.Type,
                        ReferenceId: cmd.ReferenceId,
                        ReferencePreview: cmd.ReferencePreview,
                        IsRead: false,
                        CreatedAt: DateTime.UtcNow)),
                GetUnreadCountQuery => Task.FromResult<object?>(3L),
                _ => throw new NotSupportedException($"Unsupported request: {request.GetType().Name}")
            };
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            if (request is CreateNotificationCommand cmd && typeof(TResponse) == typeof(NotificationDto))
            {
                var dto = new NotificationDto(
                    Id: "notif-flow",
                    RecipientUserId: cmd.RecipientUserId,
                    ActorUserId: cmd.ActorUserId,
                    ActorUsername: cmd.ActorUsername,
                    ActorAvatarUrl: cmd.ActorAvatarUrl,
                    Type: cmd.Type,
                    ReferenceId: cmd.ReferenceId,
                    ReferencePreview: cmd.ReferencePreview,
                    IsRead: false,
                    CreatedAt: DateTime.UtcNow);

                return Task.FromResult((TResponse)(object)dto);
            }

            if (request is GetUnreadCountQuery && typeof(TResponse) == typeof(long))
            {
                return Task.FromResult((TResponse)(object)3L);
            }

            throw new NotSupportedException($"Unsupported request: {request.GetType().Name}");
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FlowPushService : INotificationPushService
    {
        public List<NotificationDto> PushedNotifications { get; } = [];
        public List<(string UserId, long Count)> UnreadCountPushes { get; } = [];

        public Task PushNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            PushedNotifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task PushUnreadCountAsync(string userId, long count, CancellationToken cancellationToken = default)
        {
            UnreadCountPushes.Add((userId, count));
            return Task.CompletedTask;
        }
    }
}
