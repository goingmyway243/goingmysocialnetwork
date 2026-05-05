namespace GoingMy.Post.Domain.Repositories;

/// <summary>Aggregate statistics returned by <see cref="IPostRepository.GetStatsAsync"/>.</summary>
public record PostStats(
    long TotalPosts,
    long TotalLikes,
    long TotalComments,
    long PostsLast7Days,
    long PostsLast30Days);

/// <summary>
/// Interface for Post repository operations.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Retrieves all posts.
    /// </summary>
    Task<IEnumerable<Entities.Post>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a post by ID.
    /// </summary>
    Task<Entities.Post?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new post.
    /// </summary>
    Task<Entities.Post> AddAsync(Entities.Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing post.
    /// </summary>
    Task<Entities.Post> UpdateAsync(Entities.Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a post by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically increments the Likes counter of a post by 1.
    /// </summary>
    Task IncrementLikesAsync(string postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically decrements the Likes counter of a post by 1 (minimum 0).
    /// </summary>
    Task DecrementLikesAsync(string postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically increments the Comments counter of a post by 1.
    /// </summary>
    Task IncrementCommentsAsync(string postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically decrements the Comments counter of a post by 1 (minimum 0).
    /// </summary>
    Task DecrementCommentsAsync(string postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-updates the denormalized Author fields on all posts belonging to <paramref name="userId"/>.
    /// Used to propagate user profile changes (username, avatar, verification) across all existing posts.
    /// </summary>
    Task<long> BulkUpdateAuthorAsync(
        string userId,
        string username,
        string firstName,
        string lastName,
        string? avatarUrl,
        bool isVerified,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all posts belonging to <paramref name="userId"/> as authored by a deleted user.
    /// </summary>
    Task<long> MarkPostsAsDeletedUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of posts authored by the specified user.
    /// </summary>
    Task<IEnumerable<Entities.Post>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves posts whose IDs are in the provided collection. Used for liked-posts lookups.
    /// </summary>
    Task<IEnumerable<Entities.Post>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns aggregate statistics for admin reporting: total counts and per-day post counts for the last 30 days.
    /// </summary>
    Task<PostStats> GetStatsAsync(CancellationToken cancellationToken = default);
}

