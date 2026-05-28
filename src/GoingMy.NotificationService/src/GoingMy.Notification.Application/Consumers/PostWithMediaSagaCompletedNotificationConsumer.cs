using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Commands;
using GoingMy.Notification.Domain.Enums;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;

namespace GoingMy.Notification.Application.Consumers;

public class PostWithMediaSagaCompletedNotificationConsumer(
    IMediator mediator,
    INotificationPushService pushService)
    : IConsumer<PostWithMediaSagaCompletedEvent>
{
    public async Task Consume(ConsumeContext<PostWithMediaSagaCompletedEvent> context)
    {
        var evt = context.Message;

        var notificationType = evt.IsSuccess
            ? NotificationType.PostWithMediaCreated
            : NotificationType.PostWithMediaFailed;

        var referencePreview = evt.IsSuccess
            ? "Your post with attachments is ready."
            : $"Post with attachments failed: {evt.ErrorMessage}";

        var dto = await mediator.Send(new CreateNotificationCommand(
            RecipientUserId: evt.UserId,
            ActorUserId: evt.UserId,
            ActorUsername: "GoingMySocial",
            ActorAvatarUrl: null,
            Type: notificationType,
            ReferenceId: evt.PostId,
            ReferencePreview: referencePreview),
            context.CancellationToken);

        await pushService.PushNotificationAsync(dto, context.CancellationToken);

        var unreadCount = await mediator.Send(
            new Queries.GetUnreadCountQuery(dto.RecipientUserId),
            context.CancellationToken);

        await pushService.PushUnreadCountAsync(dto.RecipientUserId, unreadCount, context.CancellationToken);
    }
}
