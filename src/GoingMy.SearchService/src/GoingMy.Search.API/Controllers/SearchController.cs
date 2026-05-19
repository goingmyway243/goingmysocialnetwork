using GoingMy.Search.API.Dtos;
using GoingMy.Search.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.Search.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController(ISearchService searchService) : ControllerBase
{
    /// <summary>
    /// Search for users and/or posts with optional filters and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string q = "",
        [FromQuery] string type = "all",
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? location = null,
        [FromQuery] string sortBy = "relevance",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var filters = new SearchFilters(type, dateFrom, dateTo, location, sortBy);

        return type.ToLowerInvariant() switch
        {
            "users" => Ok(await searchService.SearchUsersAsync(q, filters, page, pageSize, ct)),
            "posts" => Ok(await searchService.SearchPostsAsync(q, filters, page, pageSize, ct)),
            _ => Ok(await SearchAllResult(q, filters, page, pageSize, ct))
        };
    }

    /// <summary>
    /// Get autocomplete suggestions for users or posts.
    /// </summary>
    [HttpGet("suggest")]
    [ProducesResponseType(typeof(IReadOnlyList<SuggestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Suggest(
        [FromQuery] string q,
        [FromQuery] string? type = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<SuggestionDto>());

        var suggestions = await searchService.SuggestAsync(q, type, ct);
        return Ok(suggestions);
    }

    /// <summary>
    /// Get trending posts within a specified time window.
    /// </summary>
    [HttpGet("trending")]
    [ProducesResponseType(typeof(IReadOnlyList<TrendingPostDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Trending(
        [FromQuery] string timeWindow = "week",
        [FromQuery] int size = 10,
        CancellationToken ct = default)
    {
        if (size is < 1 or > 50) size = 10;
        var trending = await searchService.GetTrendingAsync(timeWindow, size, ct);
        return Ok(trending);
    }

    private async Task<object> SearchAllResult(
        string q, SearchFilters filters, int page, int pageSize, CancellationToken ct)
    {
        var (users, posts) = await searchService.SearchAllAsync(q, filters, page, pageSize, ct);
        return new { Users = users, Posts = posts };
    }
}
