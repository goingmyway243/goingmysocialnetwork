using GoingMy.Post.Domain.Entities;
using GoingMy.Post.Domain.Repositories;
using GoingMy.Post.Infrastructure.Data;
using MongoDB.Driver;

namespace GoingMy.Post.Infrastructure.Repositories;

public class LikeRepository(MongoDbContext context) : ILikeRepository
{
    public async Task<bool> ExistsAsync(string postId, string userId, CancellationToken cancellationToken = default)
    {
        var count = await context.Likes
            .CountDocumentsAsync(
                Builders<Like>.Filter.And(
                    Builders<Like>.Filter.Eq(l => l.PostId, postId),
                    Builders<Like>.Filter.Eq(l => l.UserId, userId)),
                cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<IEnumerable<Like>> GetByPostIdAsync(string postId, CancellationToken cancellationToken = default)
    {
        return await context.Likes
            .Find(l => l.PostId == postId)
            .SortByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Like> AddAsync(Like like, CancellationToken cancellationToken = default)
    {
        await context.Likes.InsertOneAsync(like, cancellationToken: cancellationToken);
        return like;
    }

    public async Task<bool> DeleteAsync(string postId, string userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Like>.Filter.And(
            Builders<Like>.Filter.Eq(l => l.PostId, postId),
            Builders<Like>.Filter.Eq(l => l.UserId, userId));
        var result = await context.Likes.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }
}
