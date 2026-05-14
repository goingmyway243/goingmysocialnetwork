using GoingMy.Chat.Application.Dtos;

namespace GoingMy.Chat.Application.Services;

/// <summary>
/// Service responsible for AI-powered chat using a local LLM via Ollama.
/// </summary>
public interface IAiChatService
{
    /// <summary>
    /// Streams AI response tokens for a given user message, using the conversation history as context.
    /// The caller is responsible for accumulating tokens and calling <see cref="SaveAiResponseAsync"/> when done.
    /// </summary>
    IAsyncEnumerable<string> StreamAiResponseAsync(
        string conversationId,
        string userMessageContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the completed AI response text as a message in the conversation.
    /// </summary>
    Task<MessageDto> SaveAiResponseAsync(
        string conversationId,
        string fullContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Non-streaming path: gets the complete AI response and saves it in one call (used by REST endpoint).
    /// </summary>
    Task<MessageDto> GetAndSaveAiResponseAsync(
        string conversationId,
        string userMessageContent,
        CancellationToken cancellationToken = default);
}
