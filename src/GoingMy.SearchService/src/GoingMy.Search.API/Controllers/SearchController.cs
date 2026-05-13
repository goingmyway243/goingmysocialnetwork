using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.Search.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    [HttpGet]
    public IActionResult Search([FromQuery] string query)
    {
        // Placeholder implementation - replace with actual search logic
        var results = new[]
        {
            new { Id = Guid.NewGuid(), Type = "Post", Content = $"Result for '{query}' #1" },
            new { Id = Guid.NewGuid(), Type = "User", Content = $"Result for '{query}' #2" },
            new { Id = Guid.NewGuid(), Type = "Post", Content = $"Result for '{query}' #3" }
        };

        return Ok(results);
    }
}
