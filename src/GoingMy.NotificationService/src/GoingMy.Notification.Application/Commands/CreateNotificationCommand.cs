using GoingMy.Notification.Application.Dtos;
using GoingMy.Notification.Domain.Enums;
using GoingMy.Notification.Domain.Repositories;
using MassTransit;
using MediatR;
using NotificationEntity = GoingMy.Notification.Domain.Entities.Notification;

namespace GoingMy.Notification.Application.Commands;

public record CreateNotificationCommand(
    string RecipientUserId,
    string ActorUserId,
    string ActorUsername,
    string? ActorAvatarUrl,
    NotificationType Type,
    string? ReferenceId,
    string? ReferencePreview) : IRequest<NotificationDto>;

public class CreateNotificationCommandHandler(INotificationRepository repository)
    : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new NotificationEntity(
            id: NewId.NextSequentialGuid().ToString(),
            recipientUserId: request.RecipientUserId,
            actorUserId: request.ActorUserId,
            actorUsername: request.ActorUsername,
            type: request.Type,
            createdAt: DateTime.UtcNow,
            actorAvatarUrl: request.ActorAvatarUrl,
            referenceId: request.ReferenceId,
            referencePreview: request.ReferencePreview);

        await repository.AddAsync(notification, cancellationToken);

        return MapToDto(notification);
    }

    internal static NotificationDto MapToDto(NotificationEntity n) =>
        new(n.Id, n.RecipientUserId, n.ActorUserId, n.ActorUsername, n.ActorAvatarUrl,
            n.Type, n.ReferenceId, n.ReferencePreview, n.IsRead, n.CreatedAt);
}
