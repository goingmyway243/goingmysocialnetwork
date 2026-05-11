using GoingMy.Notification.Application.Dtos;

namespace GoingMy.Notification.Application.Abstractions;

public interface INotificationPushService
{
    Task PushNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);
    Task PushUnreadCountAsync(string userId, long count, CancellationToken cancellationToken = default);
}
