using GoingMy.User.Domain.Entities;

namespace GoingMy.User.Domain.Repositories;

public interface IUserFollowRepository
{
    Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetFollowingIdsFromSetAsync(Guid followerId, IEnumerable<Guid> candidateIds, CancellationToken ct = default);
    Task CreateAsync(UserFollow follow, CancellationToken ct = default);
    Task DeleteAsync(Guid followerId, Guid followeeId, CancellationToken ct = default);
}
