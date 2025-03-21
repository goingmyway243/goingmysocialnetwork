using Microsoft.AspNetCore.Identity;
using Social.UserService.Domain.Enums;

namespace Social.UserService.Domain.Entities
{
  public class UserEntity : IdentityUser<Guid>
  {
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime? DateOfBirth { get; set; }
    public string? FullName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
  }
}
