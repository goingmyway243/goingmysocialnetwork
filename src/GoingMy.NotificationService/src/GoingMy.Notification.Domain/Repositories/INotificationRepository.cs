using NotificationEntity = GoingMy.Notification.Domain.Entities.Notification;

namespace GoingMy.Notification.Domain.Repositories;

public interface INotificationRepository
{
    Task<NotificationEntity> AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
    Task<NotificationEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<NotificationEntity> Items, bool HasMore)> GetByRecipientPagedAsync(
        string recipientUserId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<long> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(string id, string recipientUserId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string recipientUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string recipientUserId, CancellationToken cancellationToken = default);
}
