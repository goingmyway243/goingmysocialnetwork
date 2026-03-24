using GoingMy.Auth.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GoingMy.Auth.API.Dtos;
using GoingMy.Auth.API.Enums;
using GoingMy.Auth.API.Models;

namespace GoingMy.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(
                request.Username,
                request.Password,
                request.Email,
                request.FirstName,
                request.LastName,
                [nameof(UserRole.User)]
            );

            _logger.LogInformation("User created successfully: {Username}", request.Username);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                MapToUserResponse(user)
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User creation failed: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(new { message = "Failed to create user account" });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id.ToString());

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(MapToUserResponse(user));
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(MapToUserResponse(user));
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var dto = new UpdateUserDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Bio = request.Bio,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Location = request.Location,
                WebsiteUrl = request.WebsiteUrl,
                IsPrivate = request.IsPrivate
            };

            var user = await _userService.UpdateUserAsync(id.ToString(), dto);

            _logger.LogInformation("User updated successfully: {UserId}", id);

            return Ok(MapToUserResponse(user));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return BadRequest(new { message = "Failed to update user" });
        }
    }

    /// <summary>
    /// Deactivate user account (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeactivateUserAsync(id.ToString());
            _logger.LogInformation("User deactivated successfully: {UserId}", id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user");
            return BadRequest(new { message = "Failed to deactivate user" });
        }
    }

    /// <summary>
    /// Change user avatar (placeholder - will be implemented later)
    /// </summary>
    [HttpPost("{id:guid}/avatar")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeAvatar(Guid id, [FromBody] ChangeAvatarRequest request)
    {
        // TODO: Implement avatar upload and processing logic
        // This will include:
        // - File validation (size, format)
        // - Image resizing/optimization
        // - Upload to storage service (e.g., Azure Blob Storage, AWS S3)
        // - Update user's AvatarUrl

        await Task.CompletedTask;
        return Ok(new { message = "Avatar change endpoint - To be implemented" });
    }

    /// <summary>
    /// Change user cover photo (placeholder - will be implemented later)
    /// </summary>
    [HttpPost("{id:guid}/cover")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeCover(Guid id, [FromBody] ChangeCoverRequest request)
    {
        // TODO: Implement cover photo upload and processing logic
        // Similar to avatar upload

        await Task.CompletedTask;
        return Ok(new { message = "Cover photo change endpoint - To be implemented" });
    }

    /// <summary>
    /// Change user password (placeholder - will be implemented later)
    /// </summary>
    [HttpPost("{id:guid}/change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        // TODO: Implement password change logic
        // This will include:
        // - Verify current password
        // - Validate new password strength
        // - Hash new password using ASP.NET Core Identity's IPasswordHasher
        // - Update user's PasswordHash

        await Task.CompletedTask;
        return Ok(new { message = "Password change endpoint - To be implemented" });
    }

    private static UserResponse MapToUserResponse(ApplicationUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Location = user.Location,
            WebsiteUrl = user.WebsiteUrl,
            FollowersCount = user.FollowersCount,
            FollowingCount = user.FollowingCount,
            PostsCount = user.PostsCount,
            IsVerified = user.IsVerified,
            IsPrivate = user.IsPrivate,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
