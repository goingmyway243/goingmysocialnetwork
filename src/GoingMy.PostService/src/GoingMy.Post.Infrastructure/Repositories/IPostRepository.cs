using GoingMy.Post.Domain;

namespace GoingMy.Post.Infrastructure.Repositories;

/// <summary>
/// Interface for Post repository operations.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Retrieves all posts.
    /// </summary>
    Task<IEnumerable<Domain.Post>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a post by ID.
    /// </summary>
    Task<Domain.Post?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new post.
    /// </summary>
    Task<Domain.Post> AddAsync(Domain.Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing post.
    /// </summary>
    Task<Domain.Post> UpdateAsync(Domain.Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a post by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
