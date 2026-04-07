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
}
