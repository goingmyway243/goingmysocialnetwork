using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Commands;
using GoingMy.Notification.Domain.Enums;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;

namespace GoingMy.Notification.Application.Consumers;

public class PostLikedNotificationConsumer(
    IMediator mediator,
    INotificationPushService pushService)
    : IConsumer<PostLikedEvent>
{
    public async Task Consume(ConsumeContext<PostLikedEvent> context)
    {
        var evt = context.Message;

        // Prevent self-notifications
        if (evt.LikerUserId == evt.PostAuthorUserId)
            return;

        var dto = await mediator.Send(new CreateNotificationCommand(
            RecipientUserId: evt.PostAuthorUserId,
            ActorUserId: evt.LikerUserId,
            ActorUsername: evt.LikerUsername,
            ActorAvatarUrl: evt.LikerAvatarUrl,
            Type: NotificationType.PostLiked,
            ReferenceId: evt.PostId,
            ReferencePreview: null),
            context.CancellationToken);

        await pushService.PushNotificationAsync(dto, context.CancellationToken);

        var unreadCount = await mediator.Send(
            new Queries.GetUnreadCountQuery(dto.RecipientUserId),
            context.CancellationToken);

        await pushService.PushUnreadCountAsync(dto.RecipientUserId, unreadCount, context.CancellationToken);
    }
}
