using GoingMy.Notification.API.Hubs;
using GoingMy.Notification.Application.Abstractions;
using GoingMy.Notification.Application.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace GoingMy.Notification.API.Services;

public class NotificationPushService(IHubContext<NotificationHub> hubContext) : INotificationPushService
{
    public async Task PushNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients
            .Group(notification.RecipientUserId)
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }

    public async Task PushUnreadCountAsync(string userId, long count, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients
            .Group(userId)
            .SendAsync("UnreadCountUpdated", count, cancellationToken);
    }
}
