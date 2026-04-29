using GoingMy.User.Domain.Entities;
using GoingMy.User.Domain.Repositories;
using GoingMy.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GoingMy.User.Infrastructure.Repositories;

public class UserBlockRepository(UserDbContext context) : IUserBlockRepository
{
    public async Task<bool> ExistsAsync(Guid blockerId, Guid blockeeId, CancellationToken ct = default)
        => await context.UserBlocks.AnyAsync(b => b.BlockerId == blockerId && b.BlockeeId == blockeeId, ct);

    public async Task CreateAsync(UserBlock block, CancellationToken ct = default)
    {
        context.UserBlocks.Add(block);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid blockerId, Guid blockeeId, CancellationToken ct = default)
    {
        var block = await context.UserBlocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockeeId == blockeeId, ct);

        if (block is not null)
        {
            context.UserBlocks.Remove(block);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<IEnumerable<Guid>> GetBlockedUserIdsAsync(Guid blockerId, CancellationToken ct = default)
        => await context.UserBlocks
            .Where(b => b.BlockerId == blockerId)
            .Select(b => b.BlockeeId)
            .ToListAsync(ct);
}
