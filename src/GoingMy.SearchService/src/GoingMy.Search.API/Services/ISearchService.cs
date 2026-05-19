using GoingMy.Search.API.Dtos;

namespace GoingMy.Search.API.Services;

public interface ISearchService
{
    Task<SearchResultDto<UserSearchResultDto>> SearchUsersAsync(
        string query, SearchFilters filters, int page, int pageSize, CancellationToken ct = default);

    Task<SearchResultDto<PostSearchResultDto>> SearchPostsAsync(
        string query, SearchFilters filters, int page, int pageSize, CancellationToken ct = default);

    Task<(IReadOnlyList<UserSearchResultDto> Users, IReadOnlyList<PostSearchResultDto> Posts)> SearchAllAsync(
        string query, SearchFilters filters, int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<SuggestionDto>> SuggestAsync(
        string prefix, string? type = null, CancellationToken ct = default);

    Task<IReadOnlyList<TrendingPostDto>> GetTrendingAsync(
        string timeWindow, int size = 10, CancellationToken ct = default);
}
