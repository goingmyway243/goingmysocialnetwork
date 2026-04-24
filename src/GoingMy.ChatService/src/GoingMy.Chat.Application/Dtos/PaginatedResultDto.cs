namespace GoingMy.Chat.Application.Dtos;

/// <summary>
/// Wraps a paginated list of items with metadata for infinite scroll.
/// </summary>
public record PaginatedResultDto<T>(
    IEnumerable<T> Items,
    bool HasMore,
    int PageNumber,
    int PageSize
);
