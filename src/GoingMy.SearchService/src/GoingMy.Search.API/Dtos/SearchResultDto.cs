namespace GoingMy.Search.API.Dtos;

public record SearchResultDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
