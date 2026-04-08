using GoingMy.Auth.API.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GoingMy.Auth.API.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    // ── Identity ─────────────────────────────────────────────

    [Required]
    [MaxLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }

    public List<UserRole> Roles { get; set; } = [];

    // ── State ─────────────────────────────────────────────────
    // Profile data (bio, avatar, location, followers, etc.) lives in UserService.

    public bool IsActive { get; set; } = true;

    // ── Timestamps ────────────────────────────────────────────

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? LastActiveAt { get; set; }
}
