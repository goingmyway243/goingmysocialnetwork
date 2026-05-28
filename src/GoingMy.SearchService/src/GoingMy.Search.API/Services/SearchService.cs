using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using GoingMy.Search.API.Dtos;
using GoingMy.Search.API.Infrastructure;
using GoingMy.Search.API.Models;

namespace GoingMy.Search.API.Services;

public class SearchService(ElasticsearchClient esClient) : ISearchService
{
    public async Task<SearchResultDto<UserSearchResultDto>> SearchUsersAsync(
        string query, SearchFilters filters, int page, int pageSize, CancellationToken ct = default)
    {
        var from = (page - 1) * pageSize;

        var response = await esClient.SearchAsync<UserDoc>(s => s
            .Indices(ElasticsearchIndexMappings.UsersIndex)
            .From(from)
            .Size(pageSize)
            .Query(q => BuildUserQuery(q, query, filters))
            .Sort(sort => ApplySort(sort, filters.SortBy)), ct);

        if (!response.IsValidResponse) return Empty<UserSearchResultDto>(page, pageSize);

        var items = response.Documents.Select(MapUser).ToList();
        var total = (int)(response.HitsMetadata?.Total?.Match(th => th?.Value, l => l) ?? 0);
        return new SearchResultDto<UserSearchResultDto>(items, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    public async Task<SearchResultDto<PostSearchResultDto>> SearchPostsAsync(
        string query, SearchFilters filters, int page, int pageSize, CancellationToken ct = default)
    {
        var from = (page - 1) * pageSize;

        var response = await esClient.SearchAsync<PostDoc>(s => s
            .Indices(ElasticsearchIndexMappings.PostsIndex)
            .From(from)
            .Size(pageSize)
            .Query(q => BuildPostQuery(q, query, filters))
            .Sort(sort => ApplySort(sort, filters.SortBy)), ct);

        if (!response.IsValidResponse) return Empty<PostSearchResultDto>(page, pageSize);

        var items = response.Documents.Select(MapPost).ToList();
        var total = (int)(response.HitsMetadata?.Total?.Match(th => th?.Value, l => l) ?? 0);
        return new SearchResultDto<PostSearchResultDto>(items, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    public async Task<(IReadOnlyList<UserSearchResultDto> Users, IReadOnlyList<PostSearchResultDto> Posts)> SearchAllAsync(
        string query, SearchFilters filters, int page, int pageSize, CancellationToken ct = default)
    {
        var usersTask = SearchUsersAsync(query, filters, page, pageSize, ct);
        var postsTask = SearchPostsAsync(query, filters, page, pageSize, ct);
        await Task.WhenAll(usersTask, postsTask);
        return (usersTask.Result.Items, postsTask.Result.Items);
    }

    public async Task<IReadOnlyList<SuggestionDto>> SuggestAsync(
        string prefix, string? type = null, CancellationToken ct = default)
    {
        var suggestions = new List<SuggestionDto>();

        if (type is null or "users")
        {
            var userResponse = await esClient.SearchAsync<UserDoc>(s => s
                .Indices(ElasticsearchIndexMappings.UsersIndex)
                .Suggest(sug => sug
                    .Suggesters(suggesters => suggesters
                        .Add("user-suggest", fs => fs
                            .Prefix(prefix)
                            .Completion(c => c.Field(f => f.Suggest).Size(5))))), ct);

            if (userResponse.IsValidResponse)
            {
                var hits = userResponse.Suggest?["user-suggest"]
                    ?.OfType<CompletionSuggest<UserDoc>>()
                    .SelectMany(s => s.Options) ?? [];
                suggestions.AddRange(hits.Select(h => new SuggestionDto(h.Text, "user")));
            }
        }

        if (type is null or "posts")
        {
            var postResponse = await esClient.SearchAsync<PostDoc>(s => s
                .Indices(ElasticsearchIndexMappings.PostsIndex)
                .Suggest(sug => sug
                    .Suggesters(suggesters => suggesters
                        .Add("post-suggest", fs => fs
                            .Prefix(prefix)
                            .Completion(c => c.Field(f => f.Suggest).Size(5))))), ct);

            if (postResponse.IsValidResponse)
            {
                var hits = postResponse.Suggest?["post-suggest"]
                    ?.OfType<CompletionSuggest<PostDoc>>()
                    .SelectMany(s => s.Options) ?? [];
                suggestions.AddRange(hits.Select(h => new SuggestionDto(h.Text, "post")));
            }
        }

        return suggestions
            .GroupBy(s => s.Text, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    public async Task<IReadOnlyList<TrendingPostDto>> GetTrendingAsync(
        string timeWindow, int size = 10, CancellationToken ct = default)
    {
        var cutoff = timeWindow switch
        {
            "day" => DateTime.UtcNow.AddDays(-1),
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddDays(-30),
            _ => DateTime.UtcNow.AddDays(-7)
        };

        var response = await esClient.SearchAsync<PostDoc>(s => s
            .Indices(ElasticsearchIndexMappings.PostsIndex)
            .Size(size)
            .Query(q => q.Range(r => r
                .DateRange(d => d
                    .Field(f => f.CreatedAt)
                    .Gte(cutoff))))
            .Sort(sort => sort
                .Script(sc => sc
                    .Type(ScriptSortType.Number)
                    .Script(script => script
                        .Source("(doc['likes'].size() > 0 ? doc['likes'].value : 0) + (doc['comments'].size() > 0 ? doc['comments'].value : 0)"))
                    .Order(SortOrder.Desc))), ct);

        if (!response.IsValidResponse) return [];

        return response.Documents
            .Select(p => new TrendingPostDto(
                PostId: p.Id,
                UserId: p.UserId,
                Username: p.Username,
                Content: p.Content,
                Likes: p.Likes,
                Comments: p.Comments,
                EngagementScore: p.Likes + p.Comments,
                CreatedAt: p.CreatedAt,
                MediaAttachments: p.MediaAttachments))
            .ToList();
    }

    private static void BuildUserQuery(QueryDescriptor<UserDoc> q, string query, SearchFilters filters)
    {
        q.Bool(b =>
        {
            b.Must(must =>
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    must.MultiMatch(m => m
                        .Query(query)
                        .Fields(new[]
                        {
                            "username^3",
                            "firstName^2",
                            "lastName^2",
                            "bio",
                            "location"
                        })
                        .Type(TextQueryType.BestFields)
                        .Fuzziness(new Fuzziness("AUTO")));
                }
                else
                {
                    must.MatchAll(_ => { });
                }
            });

            var filterList = new List<Action<QueryDescriptor<UserDoc>>>();

            if (!string.IsNullOrWhiteSpace(filters.Location))
                filterList.Add(f => f.Term(t => t.Field(u => u.Location).Value(filters.Location)));

            if (filters.DateFrom.HasValue || filters.DateTo.HasValue)
                filterList.Add(f => f.Range(r => r.DateRange(d =>
                {
                    d.Field(u => u.CreatedAt);
                    if (filters.DateFrom.HasValue) d.Gte(filters.DateFrom.Value);
                    if (filters.DateTo.HasValue) d.Lte(filters.DateTo.Value);
                })));

            if (filterList.Count > 0)
                b.Filter(filterList.ToArray());
        });
    }

    private static void BuildPostQuery(QueryDescriptor<PostDoc> q, string query, SearchFilters filters)
    {
        q.Bool(b =>
        {
            b.Must(must =>
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    must.MultiMatch(m => m
                        .Query(query)
                        .Fields(new[] { "content^2", "username" })
                        .Type(TextQueryType.BestFields)
                        .Fuzziness(new Fuzziness("AUTO")));
                }
                else
                {
                    must.MatchAll(_ => { });
                }
            });

            var filterList = new List<Action<QueryDescriptor<PostDoc>>>();

            if (filters.DateFrom.HasValue || filters.DateTo.HasValue)
                filterList.Add(f => f.Range(r => r.DateRange(d =>
                {
                    d.Field(p => p.CreatedAt);
                    if (filters.DateFrom.HasValue) d.Gte(filters.DateFrom.Value);
                    if (filters.DateTo.HasValue) d.Lte(filters.DateTo.Value);
                })));

            if (filterList.Count > 0)
                b.Filter(filterList.ToArray());
        });
    }

    private static SortOptionsDescriptor<T> ApplySort<T>(SortOptionsDescriptor<T> sort, string sortBy)
        where T : class =>
        sortBy switch
        {
            "recent" => sort.Field("_doc", f => f.Order(SortOrder.Desc)),
            _ => sort.Score(s => s.Order(SortOrder.Desc))
        };

    private static UserSearchResultDto MapUser(UserDoc u) => new(
        Id: u.Id,
        Username: u.Username,
        FirstName: u.FirstName,
        LastName: u.LastName,
        Bio: u.Bio,
        AvatarUrl: u.AvatarUrl,
        Location: u.Location,
        FollowersCount: u.FollowersCount,
        IsVerified: u.IsVerified,
        IsPrivate: u.IsPrivate);

    private static PostSearchResultDto MapPost(PostDoc p) => new(
        Id: p.Id,
        UserId: p.UserId,
        Username: p.Username,
        Content: p.Content,
        Likes: p.Likes,
        Comments: p.Comments,
        CreatedAt: p.CreatedAt,
        MediaAttachments: p.MediaAttachments);

    private static SearchResultDto<T> Empty<T>(int page, int pageSize) =>
        new([], 0, page, pageSize, 0);
}
