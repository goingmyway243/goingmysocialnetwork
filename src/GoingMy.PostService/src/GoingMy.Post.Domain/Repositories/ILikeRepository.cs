namespace GoingMy.Post.Domain.Repositories;

public interface ILikeRepository
{
    Task<bool> ExistsAsync(string postId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Like>> GetByPostIdAsync(string postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Like>> GetByUserIdAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Entities.Like> AddAsync(Entities.Like like, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string postId, string userId, CancellationToken cancellationToken = default);
}
