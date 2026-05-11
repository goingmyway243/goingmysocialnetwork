using MongoDB.Driver;
using NotificationEntity = GoingMy.Notification.Domain.Entities.Notification;

namespace GoingMy.Notification.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<NotificationEntity> Notifications
        => _database.GetCollection<NotificationEntity>("notifications");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Index: notifications by recipient + time for inbox queries
        var recipientTimeIndex = Builders<NotificationEntity>.IndexKeys
            .Ascending(n => n.RecipientUserId)
            .Descending(n => n.CreatedAt);
        await SafeCreateIndexAsync(Notifications,
            new CreateIndexModel<NotificationEntity>(recipientTimeIndex), cancellationToken);

        // Index: notifications by recipient + isRead for unread count queries
        var recipientReadIndex = Builders<NotificationEntity>.IndexKeys
            .Ascending(n => n.RecipientUserId)
            .Ascending(n => n.IsRead);
        await SafeCreateIndexAsync(Notifications,
            new CreateIndexModel<NotificationEntity>(recipientReadIndex), cancellationToken);
    }

    private static async Task SafeCreateIndexAsync<T>(
        IMongoCollection<T> collection,
        CreateIndexModel<T> model,
        CancellationToken ct)
    {
        try
        {
            await collection.Indexes.CreateOneAsync(model, cancellationToken: ct);
        }
        catch (MongoCommandException ex) when (ex.Code == 68)
        {
            // Index already exists — expected on restart
        }
    }
}
