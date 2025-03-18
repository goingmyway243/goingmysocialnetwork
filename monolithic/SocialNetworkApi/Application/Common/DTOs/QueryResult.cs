namespace SocialNetworkApi.Application.Common.DTOs;

public class QueryResult<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; }

    public QueryResult(T? data, bool success = false, string error = "")
    {
        Data = data;
        IsSuccess = success;
        Error = error;
    }

    public static QueryResult<T> Success(T data)
    {
        return new QueryResult<T>(data, true);
    }

    public static QueryResult<T> Failure(string error)
    {
        return new QueryResult<T>(default, false, "Query failed: " + error);
    }
}
