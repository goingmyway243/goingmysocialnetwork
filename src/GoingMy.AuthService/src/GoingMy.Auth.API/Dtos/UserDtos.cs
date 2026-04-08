using System.ComponentModel.DataAnnotations;

namespace GoingMy.Auth.API.Dtos;

#region User Requests

public record SignUpRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string LastName { get; init; }
}

/// <summary>Updates auth-relevant identity fields (name used in OIDC claims).</summary>
public record UpdateUserRequest
{
    [StringLength(50, MinimumLength = 1)]
    public string? FirstName { get; init; }

    [StringLength(50, MinimumLength = 1)]
    public string? LastName { get; init; }
}

public record ChangePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string NewPassword { get; init; }
}

#endregion

#region User Service DTOs

/// <summary>Internal DTO for auth-relevant user updates.</summary>
public record UpdateUserDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

#endregion

#region User Responses

/// <summary>
/// Auth-service user response. Contains identity fields only.
/// Full profile data (bio, avatar, followers, etc.) is served by UserService.
/// </summary>
public record UserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

#endregion
