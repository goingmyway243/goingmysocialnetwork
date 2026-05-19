namespace GoingMy.Search.API.Dtos;

public record SearchFilters(
    string? Type,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? Location,
    string SortBy = "relevance");
