using GoingMy.User.Domain.Entities;
using GoingMy.User.Domain.Repositories;
using GoingMy.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GoingMy.User.Infrastructure.Repositories;

public class UserFollowRepository(UserDbContext context) : IUserFollowRepository
{
    public async Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken ct = default)
        => await context.UserFollows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);

    public async Task CreateAsync(UserFollow follow, CancellationToken ct = default)
    {
        context.UserFollows.Add(follow);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid followerId, Guid followeeId, CancellationToken ct = default)
    {
        var follow = await context.UserFollows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);

        if (follow is not null)
        {
            context.UserFollows.Remove(follow);
            await context.SaveChangesAsync(ct);
        }
    }
}
