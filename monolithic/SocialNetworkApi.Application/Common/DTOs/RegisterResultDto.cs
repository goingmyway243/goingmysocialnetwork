namespace SocialNetworkApi.Application.Common.DTOs;

public class RegisterResultDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Error { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    public RegisterResultDto(Guid userId, string userName, string error = "")
    {
        UserId = userId;
        UserName = userName;
        Error = error;
    }

    public static RegisterResultDto Success(Guid userId, string userName)
    {
        return new RegisterResultDto(userId, userName);
    }

    public static RegisterResultDto Failure(string error)
    {
        return new RegisterResultDto(default, string.Empty, error);
    }
}
