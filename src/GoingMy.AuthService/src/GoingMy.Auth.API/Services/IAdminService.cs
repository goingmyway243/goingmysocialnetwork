using GoingMy.Auth.API.Dtos;

namespace GoingMy.Auth.API.Services;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(
        int page, int pageSize, string? search, bool? isActive, CancellationToken ct = default);

    Task<AdminUserDto> SetUserActiveStatusAsync(Guid userId, bool isActive, CancellationToken ct = default);

    Task RevokeUserTokensAsync(Guid userId, CancellationToken ct = default);

    Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default);
}
