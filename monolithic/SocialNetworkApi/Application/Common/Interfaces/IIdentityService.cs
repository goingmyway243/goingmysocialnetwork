using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> CreateUserAsync(RegisterDto registerDto);
    Task<AuthResult> PasswordSignInAsync(LoginDto loginDto);
    Task SignOutAsync();
    Task<bool> IsUserInRoleAsync(string userId, UserRole role);
    Guid GetCurrentUserId();
    string GeneratePasswordHash(UserEntity user, string password);
    string GenerateJwtToken(UserEntity user);
}