namespace GoingMy.Post.Domain.Repositories;

/// <summary>
/// Interface for Post repository operations.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Retrieves all posts.
    /// </summary>
    Task<IEnumerable<Post>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a post by ID.
    /// </summary>
    Task<Post?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new post.
    /// </summary>
    Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing post.
    /// </summary>
    Task<Post> UpdateAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a post by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
