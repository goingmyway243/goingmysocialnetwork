using GoingMy.Post.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.Post.API.Controllers;

/// <summary>
/// Request DTO for the AI writing assistant.
/// </summary>
public record AiAssistApiRequest(string Action, string? Content, string? Tone);

/// <summary>
/// Response DTO from the AI writing assistant.
/// </summary>
public record AiAssistApiResponse(string Suggestion);

/// <summary>
/// API controller for AI writing assistance.
/// </summary>
[ApiController]
[Route("api/posts/ai")]
[Authorize]
public class AiAssistController : ControllerBase
{
    private readonly IAiWritingService _aiWritingService;

    public AiAssistController(IAiWritingService aiWritingService)
    {
        _aiWritingService = aiWritingService;
    }

    /// <summary>
    /// Provides an AI-generated suggestion for a social post.
    /// </summary>
    [HttpPost("assist")]
    public async Task<ActionResult<AiAssistApiResponse>> Assist(
        [FromBody] AiAssistApiRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiWritingAction>(request.Action, ignoreCase: true, out var action))
        {
            return BadRequest($"Unknown action '{request.Action}'. Valid values: suggest, improve, grammar, tone, shorten, lengthen.");
        }

        if (action != AiWritingAction.Suggest && string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required for this action.");
        }

        var assistRequest = new AiAssistRequest(action, request.Content, request.Tone);
        var suggestion = await _aiWritingService.AssistAsync(assistRequest, cancellationToken);
        return Ok(new AiAssistApiResponse(suggestion));
    }
}
