namespace SocialNetworkApi.Application.Common.DTOs;

public class AuthResultDto
{
    public UserDto? User { get; set; }
    public string Error { get; set; }

    public AuthResultDto(UserDto? user, string error = "")
    {
        User = user;
        Error = error;
    }

    public static AuthResultDto Success(UserDto? user)
    {
        return new AuthResultDto(user);
    }

    public static AuthResultDto Failure(string error)
    {
        return new AuthResultDto(null, error);
    }
}
