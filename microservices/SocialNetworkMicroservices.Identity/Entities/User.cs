using Microsoft.AspNetCore.Identity;

namespace SocialNetworkMicroservices.Identity.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
