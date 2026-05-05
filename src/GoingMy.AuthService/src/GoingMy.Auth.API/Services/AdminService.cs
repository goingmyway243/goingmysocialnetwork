using GoingMy.Auth.API.Dtos;
using GoingMy.Auth.API.Enums;
using GoingMy.Auth.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoingMy.Auth.API.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRevocationService _revocationService;

    public AdminService(UserManager<ApplicationUser> userManager, IUserRevocationService revocationService)
    {
        _userManager = userManager;
        _revocationService = revocationService;
    }

    // ── 1. User list ─────────────────────────────────────────────

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(
        int page, int pageSize, string? search, bool? isActive, CancellationToken ct = default)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u =>
                u.UserName!.ToLower().Contains(s) ||
                u.Email!.ToLower().Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s));
        }

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<AdminUserDto>(users.Select(MapToAdminUserDto), total, page, pageSize);
    }

    // ── 2. Activate / deactivate ─────────────────────────────────

    public async Task<AdminUserDto> SetUserActiveStatusAsync(Guid userId, bool isActive, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException($"User {userId} not found");

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Deactivating a user also revokes all their current tokens immediately
        if (!isActive)
            await _revocationService.RevokeUserTokensAsync(userId.ToString(), ct);

        return MapToAdminUserDto(user);
    }

    // ── 3. Revoke tokens ─────────────────────────────────────────

    public async Task RevokeUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var exists = await _userManager.Users.AnyAsync(u => u.Id == userId, ct);
        if (!exists)
            throw new KeyNotFoundException($"User {userId} not found");

        await _revocationService.RevokeUserTokensAsync(userId.ToString(), ct);
    }

    // ── 4. Stats ─────────────────────────────────────────────────

    public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default)
    {
        var users = await _userManager.Users.ToListAsync(ct);

        var cutoff = DateTime.UtcNow.AddDays(-30).Date;
        var registrations = users
            .Where(u => u.CreatedAt.Date >= cutoff)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new DailyRegistrationDto(g.Key.ToString("yyyy-MM-dd"), g.Count()))
            .OrderBy(r => r.Date)
            .ToList();

        return new UserStatsDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive),
            AdminUsers = users.Count(u => u.Roles.Contains(UserRole.Admin)),
            RegistrationsLast30Days = registrations
        };
    }

    // ── Mapping ──────────────────────────────────────────────────

    private static AdminUserDto MapToAdminUserDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        Username = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Roles = user.Roles,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt
    };
}
