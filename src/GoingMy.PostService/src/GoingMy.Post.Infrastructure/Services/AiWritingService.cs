using GoingMy.Post.Application.Services;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace GoingMy.Post.Infrastructure.Services;

/// <summary>
/// OpenAI-based implementation using Google Gemini API (OpenAI-compatible endpoint).
/// </summary>
public sealed class AiWritingService : IAiWritingService
{
    private readonly ChatClient _chatClient;

    public AiWritingService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey configuration is missing.");
        var model = configuration["OpenAI:Model"] ?? "gemini-1.5-flash";
        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/openai/";

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(baseUrl)
        };

        var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        _chatClient = client.GetChatClient(model);
    }

    /// <inheritdoc />
    public async Task<string> AssistAsync(AiAssistRequest request, CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildSystemPrompt(request);
        var userMessage = BuildUserMessage(request);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return completion.Value.Content[0].Text.Trim();
    }

    private static string BuildSystemPrompt(AiAssistRequest request)
    {
        return request.Action switch
        {
            AiWritingAction.Suggest =>
                "You are a creative social media assistant. Generate an engaging, authentic social media post idea. " +
                "Keep it concise (under 280 characters ideally), natural, and suitable for a professional social network. " +
                "Return ONLY the post text, nothing else.",

            AiWritingAction.Improve =>
                "You are a social media writing coach. Rewrite the given post to make it more engaging, clear, and compelling. " +
                "Maintain the original intent and voice. Return ONLY the improved post text.",

            AiWritingAction.Grammar =>
                "You are a grammar and spelling editor. Fix any grammar, spelling, and punctuation errors. " +
                "Do not change the style, tone, or meaning. Return ONLY the corrected text.",

            AiWritingAction.Tone when request.Tone is not null =>
                $"Rewrite the post in a {request.Tone.ToLower().Replace(" ", "_")} tone. " +
                "Preserve meaning and key points. Return ONLY the rewritten post.",

            AiWritingAction.Shorten =>
                "Shorten the post while preserving its key message. Aim for at least 30% reduction. " +
                "Return ONLY the shortened post text.",

            AiWritingAction.Lengthen =>
                "Expand the post by adding relevant details and context. Keep it natural. " +
                "Return ONLY the expanded post text.",

            _ => "Help improve the given post. Return ONLY the improved text."
        };
    }

    private static string BuildUserMessage(AiAssistRequest request)
    {
        if (request.Action == AiWritingAction.Suggest)
        {
            return string.IsNullOrWhiteSpace(request.Content)
                ? "Generate an interesting social media post."
                : $"Generate a social media post inspired by this topic: {request.Content}";
        }

        return string.IsNullOrWhiteSpace(request.Content)
            ? "Please provide some text to work with."
            : request.Content;
    }
}
