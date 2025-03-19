using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Domain.Entities;

public class UserEntity : AuditedEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime DateOfBirth { get; set; }
    public string? FullName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }

    public bool HasRole(UserRole role) => Role == role;
}
