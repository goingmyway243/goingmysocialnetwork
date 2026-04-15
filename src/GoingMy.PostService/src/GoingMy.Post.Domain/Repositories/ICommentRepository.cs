namespace GoingMy.Post.Domain.Repositories;

public interface ICommentRepository
{
    Task<Entities.Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Comment>> GetByPostIdAsync(string postId, CancellationToken cancellationToken = default);
    Task<Entities.Comment> AddAsync(Entities.Comment comment, CancellationToken cancellationToken = default);
    Task<Entities.Comment> UpdateAsync(Entities.Comment comment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
