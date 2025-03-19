namespace SocialNetworkApi.Application.Common.DTOs;

public class AuthResult
{
    public UserDto? User { get; set; }
    public string Error { get; set; }

    public AuthResult(UserDto? user, string error = "")
    {
        User = user;
        Error = error;
    }

    public static AuthResult Success(UserDto? user)
    {
        return new AuthResult(user);
    }

    public static AuthResult Failure(string error)
    {
        return new AuthResult(null, error);
    }
}
