using GoingMy.Auth.API.Enums;
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

public record UpdateUserRequest
{
    [StringLength(50, MinimumLength = 1)]
    public string? FirstName { get; init; }

    [StringLength(50, MinimumLength = 1)]
    public string? LastName { get; init; }

    [StringLength(500)]
    public string? Bio { get; init; }

    public DateTime? DateOfBirth { get; init; }

    public Gender? Gender { get; init; }

    [StringLength(100)]
    public string? Location { get; init; }

    [StringLength(200)]
    [Url]
    public string? WebsiteUrl { get; init; }

    public bool? IsPrivate { get; init; }
}

public record ChangeAvatarRequest
{
    [Required]
    public required string AvatarUrl { get; init; }
}

public record ChangeCoverRequest
{
    [Required]
    public required string CoverUrl { get; init; }
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

public record UpdateUserDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Bio { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    public string? Location { get; init; }
    public string? WebsiteUrl { get; init; }
    public bool? IsPrivate { get; init; }
}

#endregion

#region User Responses

public record UserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverUrl { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string? Location { get; init; }
    public string? WebsiteUrl { get; init; }
    public int FollowersCount { get; init; }
    public int FollowingCount { get; init; }
    public int PostsCount { get; init; }
    public bool IsVerified { get; init; }
    public bool IsPrivate { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

#endregion
