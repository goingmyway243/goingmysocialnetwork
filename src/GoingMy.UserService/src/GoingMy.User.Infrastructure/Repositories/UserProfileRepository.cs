using GoingMy.User.Domain.Entities;
using GoingMy.User.Domain.Repositories;
using GoingMy.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GoingMy.User.Infrastructure.Repositories;

public class UserProfileRepository(UserDbContext context) : IUserProfileRepository
{
    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.UserProfiles.FindAsync([id], ct);

    public async Task<UserProfile> CreateAsync(UserProfile profile, CancellationToken ct = default)
    {
        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync(ct);
        return profile;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        context.UserProfiles.Update(profile);
        await context.SaveChangesAsync(ct);
        return profile;
    }

    public async Task<IEnumerable<UserProfile>> GetFollowersAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var followerIds = await context.UserFollows
            .Where(f => f.FolloweeId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.FollowerId)
            .ToListAsync(ct);

        return await context.UserProfiles
            .Where(p => followerIds.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<UserProfile>> GetFollowingAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var followingIds = await context.UserFollows
            .Where(f => f.FollowerId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.FolloweeId)
            .ToListAsync(ct);

        return await context.UserProfiles
            .Where(p => followingIds.Contains(p.Id))
            .ToListAsync(ct);
    }
}
