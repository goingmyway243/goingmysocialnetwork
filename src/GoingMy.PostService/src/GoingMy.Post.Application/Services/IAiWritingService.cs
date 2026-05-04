namespace GoingMy.Post.Application.Services;

/// <summary>
/// AI actions available for the writing assistant.
/// </summary>
public enum AiWritingAction
{
    Suggest,
    Improve,
    Grammar,
    Tone,
    Shorten,
    Lengthen
}

/// <summary>
/// Request model for the AI writing assistant.
/// </summary>
public record AiAssistRequest(
    AiWritingAction Action,
    string? Content,
    string? Tone = null
);

/// <summary>
/// Provides AI-powered writing assistance for social posts.
/// Supports Google Gemini models via OpenAI-compatible API.
/// </summary>
public interface IAiWritingService
{
    /// <summary>
    /// Processes an AI writing request and returns a suggested text.
    /// </summary>
    Task<string> AssistAsync(AiAssistRequest request, CancellationToken cancellationToken = default);
}
