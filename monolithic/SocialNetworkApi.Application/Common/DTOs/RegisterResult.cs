namespace SocialNetworkApi.Application.Common.DTOs;

public class RegisterResult
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Error { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    public RegisterResult(Guid userId, string userName, string error = "")
    {
        UserId = userId;
        UserName = userName;
        Error = error;
    }

    public static RegisterResult Success(Guid userId, string userName)
    {
        return new RegisterResult(userId, userName);
    }

    public static RegisterResult Failure(string error)
    {
        return new RegisterResult(default, string.Empty, error);
    }
}
