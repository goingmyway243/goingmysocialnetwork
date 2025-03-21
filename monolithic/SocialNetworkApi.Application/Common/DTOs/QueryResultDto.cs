namespace SocialNetworkApi.Application.Common.DTOs;

public class QueryResultDto<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }

    public QueryResultDto(T? data, bool success = false, string error = "")
    {
        Data = data;
        IsSuccess = success;
        Error = error;
    }

    public static QueryResultDto<T> Success(T data)
    {
        return new QueryResultDto<T>(data, true);
    }

    public static QueryResultDto<T> Failure(string error)
    {
        return new QueryResultDto<T>(default, false, "Query failed: " + error);
    }
}
