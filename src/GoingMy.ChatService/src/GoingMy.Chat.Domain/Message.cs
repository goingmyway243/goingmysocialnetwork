namespace GoingMy.Chat.Domain;

/// <summary>
/// Represents a single message within a conversation.
/// </summary>
public class Message
{
    public string Id { get; set; } = null!;
    public string ConversationId { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string SenderUsername { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? EditedContent { get; set; }
    public DateTime? EditedAt { get; set; }

    public Message(string id, string conversationId, string senderId, string senderUsername, string content, DateTime sentAt)
    {
        Id = id;
        ConversationId = conversationId;
        SenderId = senderId;
        SenderUsername = senderUsername;
        Content = content;
        SentAt = sentAt;
    }

    /// <summary>
    /// Private constructor for MongoDB.
    /// </summary>
    private Message() { }

    /// <summary>
    /// Soft-deletes this message.
    /// </summary>
    public void Delete() => IsDeleted = true;

    /// <summary>
    /// Edits the message content. Only the original sender should call this.
    /// </summary>
    public void Edit(string newContent)
    {
        EditedContent = newContent;
        EditedAt = DateTime.UtcNow;
    }
}
