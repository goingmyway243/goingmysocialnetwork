using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Commands;
using GoingMy.Notification.Application.Consumers;
using GoingMy.Notification.Application.Dtos;
using GoingMy.Notification.Application.Queries;
using GoingMy.Notification.Domain.Enums;
using GoingMy.Shared.Events;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GoingMy.Notification.Tests;

public class PostWithMediaSagaCompletedNotificationConsumerTests
{
    [Fact]
    public async Task Should_create_and_push_success_notification()
    {
        var mediator = new FakeMediator();
        var pushService = new FakePushService();

        await using var provider = new ServiceCollection()
            .AddSingleton<IMediator>(mediator)
            .AddSingleton<INotificationPushService>(pushService)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PostWithMediaSagaCompletedNotificationConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new PostWithMediaSagaCompletedEvent
            {
                CorrelationId = Guid.NewGuid(),
                UserId = "user-1",
                PostId = "post-1",
                IsSuccess = true,
                ErrorMessage = null
            });

            Assert.True(await harness.Consumed.Any<PostWithMediaSagaCompletedEvent>());
            Assert.Contains(mediator.Requests, r => r is CreateNotificationCommand cmd && cmd.Type == NotificationType.PostWithMediaCreated);
            Assert.Contains(mediator.Requests, r => r is GetUnreadCountQuery q && q.UserId == "user-1");
            Assert.Single(pushService.PushedNotifications);
            Assert.Equal(NotificationType.PostWithMediaCreated, pushService.PushedNotifications[0].Type);
            Assert.Single(pushService.UnreadCountPushes);
            Assert.Equal("user-1", pushService.UnreadCountPushes[0].UserId);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Should_create_and_push_failed_notification()
    {
        var mediator = new FakeMediator();
        var pushService = new FakePushService();

        await using var provider = new ServiceCollection()
            .AddSingleton<IMediator>(mediator)
            .AddSingleton<INotificationPushService>(pushService)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PostWithMediaSagaCompletedNotificationConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new PostWithMediaSagaCompletedEvent
            {
                CorrelationId = Guid.NewGuid(),
                UserId = "user-2",
                PostId = null,
                IsSuccess = false,
                ErrorMessage = "failed reason"
            });

            Assert.True(await harness.Consumed.Any<PostWithMediaSagaCompletedEvent>());
            Assert.Contains(mediator.Requests, r => r is CreateNotificationCommand cmd && cmd.Type == NotificationType.PostWithMediaFailed);
            Assert.Single(pushService.PushedNotifications);
            Assert.Equal(NotificationType.PostWithMediaFailed, pushService.PushedNotifications[0].Type);
            Assert.Contains("failed reason", pushService.PushedNotifications[0].ReferencePreview);
        }
        finally
        {
            await harness.Stop();
        }
    }

    private sealed class FakeMediator : IMediator
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
                        Id: "notif-1",
                        RecipientUserId: cmd.RecipientUserId,
                        ActorUserId: cmd.ActorUserId,
                        ActorUsername: cmd.ActorUsername,
                        ActorAvatarUrl: cmd.ActorAvatarUrl,
                        Type: cmd.Type,
                        ReferenceId: cmd.ReferenceId,
                        ReferencePreview: cmd.ReferencePreview,
                        IsRead: false,
                        CreatedAt: DateTime.UtcNow)),
                GetUnreadCountQuery => Task.FromResult<object?>(5L),
                _ => throw new NotSupportedException($"Unsupported request: {request.GetType().Name}")
            };
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            if (request is CreateNotificationCommand cmd && typeof(TResponse) == typeof(NotificationDto))
            {
                var dto = new NotificationDto(
                    Id: "notif-1",
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
                return Task.FromResult((TResponse)(object)5L);
            }

            throw new NotSupportedException($"Unsupported request: {request.GetType().Name}");
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakePushService : INotificationPushService
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
