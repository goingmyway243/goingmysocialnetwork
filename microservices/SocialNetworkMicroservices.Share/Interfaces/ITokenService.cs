namespace SocialNetworkMicroservices.Share.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(object user);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
    bool ValidateRefreshToken(string token);
}
