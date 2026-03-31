namespace GoingMy.Chat.Application.Dtos;

/// <summary>
/// Data transfer object for a conversation.
/// </summary>
public record ConversationDto(
    string Id,
    List<string> ParticipantIds,
    List<string> ParticipantUsernames,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    string? LastMessagePreview
);
