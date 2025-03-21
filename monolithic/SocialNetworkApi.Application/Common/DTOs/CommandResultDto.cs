namespace SocialNetworkApi.Application.Common.DTOs;

public class CommandResultDto<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }

    public CommandResultDto(T? data, bool success = false, string error = "")
    {
        Data = data;
        IsSuccess = success;
        Error = error;
    }

    public static CommandResultDto<T> Success(T data)
    {
        return new CommandResultDto<T>(data, true);
    }

    public static CommandResultDto<T> Failure(string error)
    {
        return new CommandResultDto<T>(default, false, "Command failed: " + error);
    }
}
