using GoingMy.Auth.API.Services;
using GoingMy.Shared;
using GoingMy.Shared.Events;
using MassTransit;
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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        IPublishEndpoint publishEndpoint,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>Register a new user account.</summary>
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
                [nameof(UserRole.User)]);

            _logger.LogInformation("User created successfully: {Username}", request.Username);

            // Publish event to RabbitMQ — UserService will consume and bootstrap the social profile.
            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RegisteredAt = user.CreatedAt
            });

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                MapToUserResponse(user));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            var message = ex.Message.Replace("Failed to create user: ", string.Empty);
            _logger.LogWarning("User creation conflict: {Message}", message);
            return Conflict(new { message });
        }
        catch (InvalidOperationException ex)
        {
            var message = ex.Message.Replace("Failed to create user: ", string.Empty);
            _logger.LogWarning("User creation failed: {Message}", message);
            return BadRequest(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating user: {Message}", ex.Message);
            return BadRequest(new { message = "Failed to create user account" });
        }
    }

    /// <summary>Get user by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id.ToString());
        if (user == null)
            return NotFound(new { message = "User not found" });
        return Ok(MapToUserResponse(user));
    }

    /// <summary>Get user by username.</summary>
    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
            return NotFound(new { message = "User not found" });
        return Ok(MapToUserResponse(user));
    }

    /// <summary>Update auth-relevant identity fields (FirstName, LastName).</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var callerId = User.FindFirst("sub")?.Value;
        if (callerId != id.ToString())
            return Forbid();

        try
        {
            var dto = new UpdateUserDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName
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

    /// <summary>Deactivate user account (soft delete).</summary>
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

    /// <summary>Change user password.</summary>
    [HttpPost("{id:guid}/change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        var callerId = User.FindFirst("sub")?.Value;
        if (callerId != id.ToString())
            return Forbid();

        // TODO: Implement password change via UserManager
        await Task.CompletedTask;
        return Ok(new { message = "Password change endpoint - implementation pending" });
    }

    private static UserResponse MapToUserResponse(ApplicationUser user) => new()
    {
        Id = user.Id,
        Username = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        LastLoginAt = user.LastLoginAt
    };
}
