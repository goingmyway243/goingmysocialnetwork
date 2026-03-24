using GoingMy.Auth.API.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GoingMy.Auth.API.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    [Required]
    [MaxLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }

    public List<UserRole> Roles { get; set; } = [];

    [MaxLength(500)]
    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(200)]
    public string? WebsiteUrl { get; set; }

    public int FollowersCount { get; set; } = 0;

    public int FollowingCount { get; set; } = 0;

    public int PostsCount { get; set; } = 0;

    public bool IsVerified { get; set; } = false;

    public bool IsPrivate { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? LastActiveAt { get; set; }
}
