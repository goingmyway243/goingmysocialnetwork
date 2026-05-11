using GoingMy.Notification.Domain.Repositories;
using GoingMy.Notification.Infrastructure.Data;
using MongoDB.Driver;
using NotificationEntity = GoingMy.Notification.Domain.Entities.Notification;

namespace GoingMy.Notification.Infrastructure.Repositories;

public class NotificationRepository(MongoDbContext context) : INotificationRepository
{
    public async Task<NotificationEntity> AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        await context.Notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);
        return notification;
    }

    public async Task<NotificationEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Notifications
            .Find(n => n.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IEnumerable<NotificationEntity> Items, bool HasMore)> GetByRecipientPagedAsync(
        string recipientUserId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = pageNumber * pageSize;
        var fetchCount = pageSize + 1;

        var items = await context.Notifications
            .Find(n => n.RecipientUserId == recipientUserId)
            .SortByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Limit(fetchCount)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return (items, hasMore);
    }

    public async Task<long> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken = default)
    {
        return await context.Notifications
            .CountDocumentsAsync(
                n => n.RecipientUserId == recipientUserId && !n.IsRead,
                cancellationToken: cancellationToken);
    }

    public async Task MarkAsReadAsync(string id, string recipientUserId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<NotificationEntity>.Filter.And(
            Builders<NotificationEntity>.Filter.Eq(n => n.Id, id),
            Builders<NotificationEntity>.Filter.Eq(n => n.RecipientUserId, recipientUserId));

        var update = Builders<NotificationEntity>.Update.Set(n => n.IsRead, true);

        var result = await context.Notifications.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"Notification '{id}' not found.");
    }

    public async Task MarkAllAsReadAsync(string recipientUserId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<NotificationEntity>.Filter.And(
            Builders<NotificationEntity>.Filter.Eq(n => n.RecipientUserId, recipientUserId),
            Builders<NotificationEntity>.Filter.Eq(n => n.IsRead, false));

        var update = Builders<NotificationEntity>.Update.Set(n => n.IsRead, true);

        await context.Notifications.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, string recipientUserId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<NotificationEntity>.Filter.And(
            Builders<NotificationEntity>.Filter.Eq(n => n.Id, id),
            Builders<NotificationEntity>.Filter.Eq(n => n.RecipientUserId, recipientUserId));

        var result = await context.Notifications.DeleteOneAsync(filter, cancellationToken: cancellationToken);

        if (result.DeletedCount == 0)
            throw new InvalidOperationException($"Notification '{id}' not found.");
    }
}
