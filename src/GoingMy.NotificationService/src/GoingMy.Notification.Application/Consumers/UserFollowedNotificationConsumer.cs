using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Commands;
using GoingMy.Notification.Domain.Enums;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;

namespace GoingMy.Notification.Application.Consumers;

public class UserFollowedNotificationConsumer(
    IMediator mediator,
    INotificationPushService pushService)
    : IConsumer<UserFollowedEvent>
{
    public async Task Consume(ConsumeContext<UserFollowedEvent> context)
    {
        var evt = context.Message;

        var dto = await mediator.Send(new CreateNotificationCommand(
            RecipientUserId: evt.FollowedUserId,
            ActorUserId: evt.FollowerUserId,
            ActorUsername: evt.FollowerUsername,
            ActorAvatarUrl: null,
            Type: NotificationType.NewFollower,
            ReferenceId: evt.FollowerUserId,
            ReferencePreview: null),
            context.CancellationToken);

        await pushService.PushNotificationAsync(dto, context.CancellationToken);

        var unreadCount = await mediator.Send(
            new Queries.GetUnreadCountQuery(dto.RecipientUserId),
            context.CancellationToken);

        await pushService.PushUnreadCountAsync(dto.RecipientUserId, unreadCount, context.CancellationToken);
    }
}
