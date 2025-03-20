using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> GetUserById(Guid id);
    Task<RegisterResult> CreateUserAsync(RegisterDto registerDto);
    Task<AuthResult> PasswordSignInAsync(LoginDto loginDto);
    Task SignOutAsync();
    Task<bool> IsUserInRoleAsync(Guid userId, UserRole role);
    string GeneratePasswordHash(string password);
}