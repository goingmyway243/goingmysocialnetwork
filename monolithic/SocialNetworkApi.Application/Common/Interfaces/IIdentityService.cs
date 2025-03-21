using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResultDto> GetUserById(Guid id);
    Task<RegisterResultDto> CreateUserAsync(RegisterRequestDto registerDto);
    Task<AuthResultDto> PasswordSignInAsync(LoginRequestDto loginDto);
    Task SignOutAsync();
    Task<bool> IsUserInRoleAsync(Guid userId, UserRole role);
    string GeneratePasswordHash(string password);
}