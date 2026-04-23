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

    public async Task<IEnumerable<UserProfile>> SearchAsync(
        string? searchTerm,
        string? location,
        bool? isVerified,
        int page,
        int pageSize,
        Guid? excludeUserId = null,
        CancellationToken ct = default)
    {
        var query = context.UserProfiles.Where(p => p.IsActive).AsQueryable();

        if (excludeUserId.HasValue)
            query = query.Where(p => p.Id != excludeUserId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Username.ToLower().Contains(term) ||
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(p => p.Location != null && p.Location.ToLower().Contains(location.ToLower()));

        if (isVerified.HasValue)
            query = query.Where(p => p.IsVerified == isVerified.Value);

        return await query
            .OrderByDescending(p => p.FollowersCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}
