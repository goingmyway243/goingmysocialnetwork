using GoingMy.User.Domain.Entities;

namespace GoingMy.User.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserProfile> CreateAsync(UserProfile profile, CancellationToken ct = default);
    Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken ct = default);
    Task<IEnumerable<UserProfile>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<UserProfile>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
