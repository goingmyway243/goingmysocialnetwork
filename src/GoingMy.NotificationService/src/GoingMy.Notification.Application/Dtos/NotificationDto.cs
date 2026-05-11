using GoingMy.Notification.Domain.Enums;

namespace GoingMy.Notification.Application.Dtos;

public record NotificationDto(
    string Id,
    string RecipientUserId,
    string ActorUserId,
    string ActorUsername,
    string? ActorAvatarUrl,
    NotificationType Type,
    string? ReferenceId,
    string? ReferencePreview,
    bool IsRead,
    DateTime CreatedAt);

public record NotificationPagedResult(
    IEnumerable<NotificationDto> Items,
    bool HasMore,
    int PageNumber,
    int PageSize);
