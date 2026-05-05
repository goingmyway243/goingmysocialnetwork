using GoingMy.Auth.API.Dtos;
using GoingMy.Auth.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.Auth.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    // ── Users ────────────────────────────────────────────────────

    /// <summary>List users with optional filters and pagination.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var result = await _adminService.GetUsersAsync(page, pageSize, search, isActive, ct);
        return Ok(result);
    }

    /// <summary>Activate or deactivate a user account. Deactivation also revokes all current tokens.</summary>
    [HttpPatch("users/{id:guid}/status")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUserStatus(
        Guid id,
        [FromBody] SetUserStatusRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var user = await _adminService.SetUserActiveStatusAsync(id, request.IsActive, ct);
            _logger.LogInformation("Admin set user {UserId} active={IsActive}", id, request.IsActive);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"User {id} not found" });
        }
    }

    /// <summary>Immediately revoke all active tokens for a user (forces re-login on next request).</summary>
    [HttpPost("users/{id:guid}/revoke-tokens")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeUserTokens(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _adminService.RevokeUserTokensAsync(id, ct);
            _logger.LogInformation("Admin revoked tokens for user {UserId}", id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"User {id} not found" });
        }
    }

    // ── Stats ────────────────────────────────────────────────────

    /// <summary>Get user statistics: totals and daily registration counts for the last 30 days.</summary>
    [HttpGet("stats/users")]
    [ProducesResponseType(typeof(UserStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserStats(CancellationToken ct = default)
    {
        var stats = await _adminService.GetUserStatsAsync(ct);
        return Ok(stats);
    }
}
