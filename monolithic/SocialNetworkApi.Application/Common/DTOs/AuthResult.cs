namespace SocialNetworkApi.Application.Common.DTOs;

public class AuthResult
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string JwtToken { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }

    public AuthResult(Guid userId, string userName, string jwtToken, bool success = false, string error = "")
    {
        UserId = userId;
        UserName = userName;
        JwtToken = jwtToken;
        IsSuccess = success;
        Error = error;
    }

    public static AuthResult Success(Guid userId, string userName, string jwtToken)
    {
        return new AuthResult(userId, userName, jwtToken, true);
    }

    public static AuthResult Failure(string error)
    {
        return new AuthResult(default, "", "", false, error);
    }
}
