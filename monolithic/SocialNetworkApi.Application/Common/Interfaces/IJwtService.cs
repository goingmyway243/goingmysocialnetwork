using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(UserDto user);
    string GenerateRefreshToken();
}
