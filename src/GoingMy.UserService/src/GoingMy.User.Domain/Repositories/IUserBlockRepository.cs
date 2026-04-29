using GoingMy.User.Domain.Entities;

namespace GoingMy.User.Domain.Repositories;

public interface IUserBlockRepository
{
    Task<bool> ExistsAsync(Guid blockerId, Guid blockeeId, CancellationToken ct = default);
    Task CreateAsync(UserBlock block, CancellationToken ct = default);
    Task DeleteAsync(Guid blockerId, Guid blockeeId, CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetBlockedUserIdsAsync(Guid blockerId, CancellationToken ct = default);
}
