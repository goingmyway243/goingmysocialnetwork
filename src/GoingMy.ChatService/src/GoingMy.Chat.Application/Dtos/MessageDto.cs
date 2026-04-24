namespace GoingMy.Chat.Application.Dtos;

/// <summary>
/// Data transfer object for a single message.
/// </summary>
public record MessageDto(
    string Id,
    string ConversationId,
    string SenderId,
    string SenderUsername,
    string Content,
    DateTime SentAt,
    bool IsDeleted,
    string? EditedContent,
    DateTime? EditedAt
);
