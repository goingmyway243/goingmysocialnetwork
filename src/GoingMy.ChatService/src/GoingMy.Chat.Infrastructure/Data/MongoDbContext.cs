using MongoDB.Driver;

namespace GoingMy.Chat.Infrastructure.Data;

/// <summary>
/// MongoDB context for Chat service.
/// Manages connections and collections for Conversation and Message aggregates.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Gets the Conversations collection.
    /// </summary>
    public IMongoCollection<Domain.Conversation> Conversations
        => _database.GetCollection<Domain.Conversation>("conversations");

    /// <summary>
    /// Gets the Messages collection.
    /// </summary>
    public IMongoCollection<Domain.Message> Messages
        => _database.GetCollection<Domain.Message>("messages");

    /// <summary>
    /// Gets the ReadReceipts collection.
    /// </summary>
    public IMongoCollection<Domain.ReadReceipt> ReadReceipts
        => _database.GetCollection<Domain.ReadReceipt>("readReceipts");

    /// <summary>
    /// Initializes MongoDB collections with required indexes.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Index: conversations by participant for fast inbox queries
        var convIndex = Builders<Domain.Conversation>.IndexKeys
            .Ascending(c => c.ParticipantIds)
            .Descending(c => c.LastMessageAt);
        await SafeCreateIndexAsync(Conversations, new CreateIndexModel<Domain.Conversation>(convIndex), cancellationToken);

        // Index: messages by conversation + time for chat history
        var msgIndex = Builders<Domain.Message>.IndexKeys
            .Ascending(m => m.ConversationId)
            .Ascending(m => m.SentAt);
        await SafeCreateIndexAsync(Messages, new CreateIndexModel<Domain.Message>(msgIndex), cancellationToken);

        // Index: read receipts by messageId + userId (unique per pair)
        var receiptIndex = Builders<Domain.ReadReceipt>.IndexKeys
            .Ascending(r => r.MessageId)
            .Ascending(r => r.ReadByUserId);
        await SafeCreateIndexAsync(ReadReceipts, new CreateIndexModel<Domain.ReadReceipt>(receiptIndex), cancellationToken);

        // Index: read receipts by conversationId for lookup
        var receiptConvIndex = Builders<Domain.ReadReceipt>.IndexKeys
            .Ascending(r => r.ConversationId);
        await SafeCreateIndexAsync(ReadReceipts, new CreateIndexModel<Domain.ReadReceipt>(receiptConvIndex), cancellationToken);
    }

    private static async Task SafeCreateIndexAsync<T>(IMongoCollection<T> collection, CreateIndexModel<T> model, CancellationToken ct)
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
