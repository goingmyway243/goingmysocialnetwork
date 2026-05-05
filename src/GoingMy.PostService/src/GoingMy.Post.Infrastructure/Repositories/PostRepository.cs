using GoingMy.Post.Domain.Repositories;
using GoingMy.Post.Infrastructure.Data;
using MongoDB.Driver;

namespace GoingMy.Post.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of Post repository.
/// Provides data access operations for Post aggregates.
/// </summary>
public class PostRepository : IPostRepository
{
    private readonly MongoDbContext _context;

    public PostRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Domain.Entities.Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Find(_ => true)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Post?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Post> AddAsync(Domain.Entities.Post post, CancellationToken cancellationToken = default)
    {
        await _context.Posts.InsertOneAsync(post, cancellationToken: cancellationToken);
        return post;
    }

    public async Task<Domain.Entities.Post> UpdateAsync(Domain.Entities.Post post, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Entities.Post>.Filter.Eq(p => p.Id, post.Id);
        var result = await _context.Posts.ReplaceOneAsync(filter, post, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Post with ID {post.Id} not found");
        }

        return post;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Entities.Post>.Filter.Eq(p => p.Id, id);
        var result = await _context.Posts.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task IncrementLikesAsync(string postId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Entities.Post>.Filter.Eq(p => p.Id, postId);
        var update = Builders<Domain.Entities.Post>.Update.Inc(p => p.Likes, 1);
        await _context.Posts.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task DecrementLikesAsync(string postId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Entities.Post>.Filter.And(
            Builders<Domain.Entities.Post>.Filter.Eq(p => p.Id, postId),
            Builders<Domain.Entities.Post>.Filter.Gt(p => p.Likes, 0));
        var update = Builders<Domain.Entities.Post>.Update.Inc(p => p.Likes, -1);
        await _context.Posts.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task IncrementCommentsAsync(string postId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Entities.Post>.Filter.Eq(p => p.Id, postId);
        var update = Builders<Domain.Entities.Post>.Update.Inc(p => p.Comments, 1);
        await _context.Posts.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task DecrementCommentsAsync(string postId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Entities.Post>.Filter.And(
            Builders<Domain.Entities.Post>.Filter.Eq(p => p.Id, postId),
            Builders<Domain.Entities.Post>.Filter.Gt(p => p.Comments, 0));
        var update = Builders<Domain.Entities.Post>.Update.Inc(p => p.Comments, -1);
        await _context.Posts.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<long> BulkUpdateAuthorAsync(
        string userId,
        string username,
        string firstName,
        string lastName,
        string? avatarUrl,
        bool isVerified,
        CancellationToken cancellationToken = default)
    {
        // Filter by nested Author.Id — MongoDB driver handles nullable Author correctly in expression trees
        var filter = Builders<Domain.Entities.Post>.Filter.Eq("Author.Id", userId);

        var update = Builders<Domain.Entities.Post>.Update
            .Set("Author.UserName", username)
            .Set("Author.FirstName", firstName)
            .Set("Author.LastName", lastName)
            .Set("Author.AvatarUrl", avatarUrl)
            .Set("Author.IsVerified", isVerified);

        var result = await _context.Posts.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount;
    }

    public async Task<long> MarkPostsAsDeletedUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Entities.Post>.Filter.Eq("Author.Id", userId);

        var b = Builders<Domain.Entities.Post>.Update;
        var update = b.Combine(
            b.Set("Author.UserName", "[deleted]"),
            b.Set("Author.FirstName", "Deleted"),
            b.Set("Author.LastName", "User"),
            b.Unset("Author.AvatarUrl"),
            b.Set("Author.IsVerified", false));

        var result = await _context.Posts.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount;
    }

    public async Task<IEnumerable<Domain.Entities.Post>> GetByUserIdAsync(
        string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Find(p => p.UserId == userId)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Post>> GetByIdsAsync(
        IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return [];

        var filter = Builders<Domain.Entities.Post>.Filter.In(p => p.Id, idList);
        return await _context.Posts
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Repositories.PostStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);
        var thirtyDaysAgo = now.AddDays(-30);

        var totalPosts = await _context.Posts.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
        var postsLast7Days = await _context.Posts.CountDocumentsAsync(
            p => p.CreatedAt >= sevenDaysAgo, cancellationToken: cancellationToken);
        var postsLast30Days = await _context.Posts.CountDocumentsAsync(
            p => p.CreatedAt >= thirtyDaysAgo, cancellationToken: cancellationToken);

        // Aggregate sum of Likes and Comments across all posts
        var aggregate = await _context.Posts
            .Aggregate()
            .Group(p => true, g => new
            {
                TotalLikes = g.Sum(p => p.Likes),
                TotalComments = g.Sum(p => p.Comments)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new Domain.Repositories.PostStats(
            TotalPosts: totalPosts,
            TotalLikes: aggregate?.TotalLikes ?? 0,
            TotalComments: aggregate?.TotalComments ?? 0,
            PostsLast7Days: postsLast7Days,
            PostsLast30Days: postsLast30Days);
    }
}
