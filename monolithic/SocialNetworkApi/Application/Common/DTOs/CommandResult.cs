namespace SocialNetworkApi.Application.Common.DTOs;

public class CommandResult<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }

    public CommandResult(T? data, bool success = false, string error = "")
    {
        Data = data;
        IsSuccess = success;
        Error = error;
    }

    public static CommandResult<T> Success(T data)
    {
        return new CommandResult<T>(data, true);
    }

    public static CommandResult<T> Failure(string error)
    {
        return new CommandResult<T>(default, false, "Command failed: " + error);
    }
}
