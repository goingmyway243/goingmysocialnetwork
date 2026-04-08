using GoingMy.User.Application.Commands;
using GoingMy.User.Application.Dtos;
using GoingMy.User.Application.Queries;
using GoingMy.User.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.User.API.Controllers;

// ── Request DTOs ──────────────────────────────────────────────

public record CreateUserProfileRequest(
    Guid Id,
    string Username,
    string FirstName,
    string LastName);

public record UpdateUserProfileRequest(
    string? FirstName,
    string? LastName,
    string? Bio,
    DateTime? DateOfBirth,
    Gender? Gender,
    string? Location,
    string? WebsiteUrl,
    bool? IsPrivate);

public record UpdateAvatarRequest(string AvatarUrl);

public record UpdateCoverRequest(string CoverUrl);

// ── Controller ────────────────────────────────────────────────

/// <summary>
/// Manages user profiles, social follow graph, avatar, and cover photo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserProfilesController(IMediator mediator) : ControllerBase
{
    // ── Profile ───────────────────────────────────────────────

    /// <summary>
    /// Bootstrap a new user profile. Called by AuthService after successful signup.
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // TODO: secure with internal service-to-service auth
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUserProfile([FromBody] CreateUserProfileRequest request)
    {
        try
        {
            var profile = await mediator.Send(new CreateUserProfileCommand(
                request.Id, request.Username, request.FirstName, request.LastName));

            return CreatedAtAction(nameof(GetUserProfile), new { id = profile.Id }, profile);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate") || ex.Message.Contains("unique"))
        {
            return Conflict(new { message = "A profile for this user already exists." });
        }
    }

    /// <summary>Gets a user profile by ID.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var profile = await mediator.Send(new GetUserProfileByIdQuery(id));

        return profile is null
            ? NotFound(new { message = $"User profile '{id}' not found." })
            : Ok(profile);
    }

    /// <summary>Updates the authenticated user's profile information.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateUserProfileRequest request)
    {
        var callerId = User.FindFirst("sub")?.Value;
        if (callerId != id.ToString())
            return Forbid();

        try
        {
            var profile = await mediator.Send(new UpdateUserProfileCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Bio,
                request.DateOfBirth,
                request.Gender,
                request.Location,
                request.WebsiteUrl,
                request.IsPrivate));

            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── Avatar & Cover ────────────────────────────────────────

    /// <summary>Updates the user's avatar URL.</summary>
    [HttpPost("{id:guid}/avatar")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvatar(Guid id, [FromBody] UpdateAvatarRequest request)
    {
        var callerId = User.FindFirst("sub")?.Value;
        if (callerId != id.ToString())
            return Forbid();

        try
        {
            var profile = await mediator.Send(new UpdateAvatarCommand(id, request.AvatarUrl));
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Updates the user's cover photo URL.</summary>
    [HttpPost("{id:guid}/cover")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCover(Guid id, [FromBody] UpdateCoverRequest request)
    {
        var callerId = User.FindFirst("sub")?.Value;
        if (callerId != id.ToString())
            return Forbid();

        try
        {
            var profile = await mediator.Send(new UpdateCoverCommand(id, request.CoverUrl));
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── Follow Graph ──────────────────────────────────────────

    /// <summary>Follow a user. The authenticated caller becomes the follower.</summary>
    [HttpPost("{id:guid}/follow")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FollowUser(Guid id)
    {
        var callerId = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(callerId, out var followerGuid))
            return Unauthorized();

        try
        {
            await mediator.Send(new FollowUserCommand(followerGuid, id));
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Unfollow a user.</summary>
    [HttpDelete("{id:guid}/follow")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfollowUser(Guid id)
    {
        var callerId = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(callerId, out var followerGuid))
            return Unauthorized();

        try
        {
            await mediator.Send(new UnfollowUserCommand(followerGuid, id));
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Gets the followers list for a user (paginated).</summary>
    [HttpGet("{id:guid}/followers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowers(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var followers = await mediator.Send(new GetUserFollowersQuery(id, page, pageSize));
        return Ok(followers);
    }

    /// <summary>Gets the following list for a user (paginated).</summary>
    [HttpGet("{id:guid}/following")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowing(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var following = await mediator.Send(new GetUserFollowingQuery(id, page, pageSize));
        return Ok(following);
    }
}
