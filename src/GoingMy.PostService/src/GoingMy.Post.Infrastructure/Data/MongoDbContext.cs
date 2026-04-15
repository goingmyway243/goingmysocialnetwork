using GoingMy.Post.Domain.Entities;
using MongoDB.Driver;

namespace GoingMy.Post.Infrastructure.Data;

/// <summary>
/// MongoDB context for Post service.
/// Manages connections and collections for the Post, Like, and Comment aggregates.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Domain.Entities.Post> Posts => _database.GetCollection<Domain.Entities.Post>("posts");
    public IMongoCollection<Like> Likes => _database.GetCollection<Like>("likes");
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // posts: compound index on (UserId, CreatedAt desc) for feed queries
        await Posts.Indexes.CreateOneAsync(
            new CreateIndexModel<Domain.Entities.Post>(
                Builders<Domain.Entities.Post>.IndexKeys.Ascending(p => p.UserId).Descending(p => p.CreatedAt)),
            cancellationToken: cancellationToken);

        // likes: unique compound index (PostId, UserId) — one like per user per post
        await Likes.Indexes.CreateOneAsync(
            new CreateIndexModel<Like>(
                Builders<Like>.IndexKeys.Ascending(l => l.PostId).Ascending(l => l.UserId),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);

        // likes: single index on PostId for GetByPostId queries
        await Likes.Indexes.CreateOneAsync(
            new CreateIndexModel<Like>(
                Builders<Like>.IndexKeys.Ascending(l => l.PostId)),
            cancellationToken: cancellationToken);

        // comments: index on PostId for GetByPostId queries
        await Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<Comment>(
                Builders<Comment>.IndexKeys.Ascending(c => c.PostId).Descending(c => c.CreatedAt)),
            cancellationToken: cancellationToken);
    }
}
