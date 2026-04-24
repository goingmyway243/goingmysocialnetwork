namespace GoingMy.Chat.Domain;

/// <summary>
/// Tracks when a user read a message in a conversation.
/// </summary>
public class ReadReceipt
{
    public string Id { get; set; } = null!;
    public string ConversationId { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public string ReadByUserId { get; set; } = null!;
    public string ReadByUsername { get; set; } = null!;
    public DateTime ReadAt { get; set; }

    public ReadReceipt(string id, string conversationId, string messageId, string readByUserId, string readByUsername, DateTime readAt)
    {
        Id = id;
        ConversationId = conversationId;
        MessageId = messageId;
        ReadByUserId = readByUserId;
        ReadByUsername = readByUsername;
        ReadAt = readAt;
    }

    private ReadReceipt() { }
}
