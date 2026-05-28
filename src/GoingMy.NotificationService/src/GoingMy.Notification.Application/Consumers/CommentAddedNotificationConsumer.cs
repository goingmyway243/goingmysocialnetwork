using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Commands;
using GoingMy.Notification.Domain.Enums;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;

namespace GoingMy.Notification.Application.Consumers;

public class CommentAddedNotificationConsumer(
    IMediator mediator,
    INotificationPushService pushService)
    : IConsumer<CommentAddedEvent>
{
    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var evt = context.Message;

        // Prevent self-notifications
        if (evt.CommenterId == evt.PostAuthorUserId)
            return;

        var dto = await mediator.Send(new CreateNotificationCommand(
            RecipientUserId: evt.PostAuthorUserId,
            ActorUserId: evt.CommenterId,
            ActorUsername: evt.CommenterUsername,
            ActorAvatarUrl: evt.CommenterAvatarUrl,
            Type: NotificationType.PostCommented,
            ReferenceId: evt.PostId,
            ReferencePreview: evt.CommentPreview),
            context.CancellationToken);

        await pushService.PushNotificationAsync(dto, context.CancellationToken);

        var unreadCount = await mediator.Send(
            new Queries.GetUnreadCountQuery(dto.RecipientUserId),
            context.CancellationToken);

        await pushService.PushUnreadCountAsync(dto.RecipientUserId, unreadCount, context.CancellationToken);
    }
}
