using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Application.Services;
using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using GoingMy.Shared;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Runtime.CompilerServices;

namespace GoingMy.Chat.Infrastructure.Services;

/// <summary>
/// Implements AI-powered chat using Google Gemini via the OpenAI-compatible API.
/// Loads conversation history as context, streams response tokens, and persists the AI reply.
/// </summary>
public class AiChatService : IAiChatService
{
    private readonly ChatClient _chatClient;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;

    private const string SystemPrompt =
        "You are a helpful and friendly AI assistant on GoingMy, a social networking platform. " +
        "You can help users with questions, creative writing, content ideas for posts, and general conversation. " +
        "Be concise, warm, and supportive. Do not fabricate facts.";

    private const int ContextMessageLimit = 20;

    public AiChatService(
        IConfiguration configuration,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey configuration is missing.");
        var model = configuration["OpenAI:Model"] ?? "gemini-2.5-flash";
        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/openai/";

        var options = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
        var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        _chatClient = client.GetChatClient(model);
    }

    // ── Streaming ────────────────────────────────────────────────

    public async IAsyncEnumerable<string> StreamAiResponseAsync(
        string conversationId,
        string userMessageContent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = await BuildContextAsync(conversationId, userMessageContent, cancellationToken);

        await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (part.Text is { Length: > 0 } token)
                    yield return token;
            }
        }
    }

    // ── Persist AI reply ─────────────────────────────────────────

    public async Task<MessageDto> SaveAiResponseAsync(
        string conversationId,
        string fullContent,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        var message = new Message(
            id: ObjectId.GenerateNewId().ToString(),
            conversationId: conversationId,
            senderId: SharedServices.AiAssistant,
            senderUsername: SharedServices.AiAssistantUsername,
            content: fullContent,
            sentAt: DateTime.UtcNow
        );

        var saved = await _messageRepository.AddAsync(message, cancellationToken);

        conversation.UpdateLastMessage(fullContent.Length > 80
            ? fullContent[..80] + "\u2026"
            : fullContent);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return ToMessageDto(saved);
    }

    // ── Non-streaming (REST fallback) ────────────────────────────

    public async Task<MessageDto> GetAndSaveAiResponseAsync(
        string conversationId,
        string userMessageContent,
        CancellationToken cancellationToken = default)
    {
        var messages = await BuildContextAsync(conversationId, userMessageContent, cancellationToken);
        var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var fullContent = completion.Value.Content[0].Text.Trim();
        return await SaveAiResponseAsync(conversationId, fullContent, cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private async Task<List<OpenAI.Chat.ChatMessage>> BuildContextAsync(
        string conversationId,
        string userMessageContent,
        CancellationToken cancellationToken)
    {
        var history = await _messageRepository.GetByConversationAsync(conversationId, cancellationToken);

        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            new SystemChatMessage(SystemPrompt)
        };

        foreach (var msg in history.TakeLast(ContextMessageLimit))
        {
            if (msg.IsDeleted) continue;
            if (msg.SenderId == SharedServices.AiAssistant)
                messages.Add(new AssistantChatMessage(msg.EditedContent ?? msg.Content));
            else
                messages.Add(new UserChatMessage(msg.EditedContent ?? msg.Content));
        }

        messages.Add(new UserChatMessage(userMessageContent));
        return messages;
    }

    private static MessageDto ToMessageDto(Message m) => new(
        m.Id,
        m.ConversationId,
        m.SenderId,
        m.SenderUsername,
        m.Content,
        m.SentAt,
        m.IsDeleted,
        m.EditedContent,
        m.EditedAt
    );
}
