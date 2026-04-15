using GoingMy.Post.Domain.Entities;
using GoingMy.Post.Domain.Repositories;
using GoingMy.Post.Infrastructure.Data;
using MongoDB.Driver;

namespace GoingMy.Post.Infrastructure.Repositories;

public class CommentRepository(MongoDbContext context) : ICommentRepository
{
    public async Task<Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Comments
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(string postId, CancellationToken cancellationToken = default)
    {
        return await context.Comments
            .Find(c => c.PostId == postId)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await context.Comments.InsertOneAsync(comment, cancellationToken: cancellationToken);
        return comment;
    }

    public async Task<Comment> UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, comment.Id);
        var result = await context.Comments.ReplaceOneAsync(filter, comment, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"Comment '{comment.Id}' not found.");

        return comment;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await context.Comments.DeleteOneAsync(c => c.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
