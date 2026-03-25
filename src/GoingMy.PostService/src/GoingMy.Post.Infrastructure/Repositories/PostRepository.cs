using GoingMy.Post.Domain;

namespace GoingMy.Post.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of Post repository for demonstration.
/// In production, this would use Entity Framework Core with a database.
/// </summary>
public class PostRepository : IPostRepository
{
    private static readonly Dictionary<int, Domain.Post> Posts = new();

    public Task<IEnumerable<Domain.Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Posts.Values.AsEnumerable());
    }

    public Task<Domain.Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Posts.TryGetValue(id, out var post);
        return Task.FromResult(post);
    }

    public Task<Domain.Post> AddAsync(Domain.Post post, CancellationToken cancellationToken = default)
    {
        Posts[post.Id] = post;
        return Task.FromResult(post);
    }

    public Task<Domain.Post> UpdateAsync(Domain.Post post, CancellationToken cancellationToken = default)
    {
        Posts[post.Id] = post;
        return Task.FromResult(post);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Posts.Remove(id));
    }
}
