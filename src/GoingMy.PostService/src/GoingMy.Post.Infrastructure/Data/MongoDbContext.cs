using MongoDB.Driver;

namespace GoingMy.Post.Infrastructure.Data;

/// <summary>
/// MongoDB context for Post service.
/// Manages connections and collections for the Post aggregate.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Gets the Posts collection.
    /// </summary>
    public IMongoCollection<Domain.Post> Posts => _database.GetCollection<Domain.Post>("posts");

    /// <summary>
    /// Initializes MongoDB collections with required indexes.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Create compound index on UserId for efficient user post queries
        var indexKeys = Builders<Domain.Post>.IndexKeys.Ascending(p => p.UserId).Descending(p => p.CreatedAt);
        var indexModel = new CreateIndexModel<Domain.Post>(indexKeys);

        try
        {
            await Posts.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
        }
        catch (MongoCommandException ex) when (ex.Code == 68)
        {
            // Index already exists - this is expected
        }
    }
}
