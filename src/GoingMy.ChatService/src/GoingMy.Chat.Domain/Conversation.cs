namespace GoingMy.Chat.Domain;

/// <summary>
/// Represents a private conversation between two users.
/// </summary>
public class Conversation
{
    public string Id { get; set; } = null!;

    /// <summary>
    /// IDs of participants in this conversation.
    /// </summary>
    public List<string> ParticipantIds { get; set; } = [];

    /// <summary>
    /// Usernames of participants (denormalized for display).
    /// </summary>
    public List<string> ParticipantUsernames { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }

    public Conversation(string id, List<string> participantIds, List<string> participantUsernames, DateTime createdAt)
    {
        Id = id;
        ParticipantIds = participantIds;
        ParticipantUsernames = participantUsernames;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Private constructor for MongoDB.
    /// </summary>
    private Conversation() { }

    /// <summary>
    /// Updates the last message preview after a new message is sent.
    /// </summary>
    public void UpdateLastMessage(string preview)
    {
        LastMessagePreview = preview;
        LastMessageAt = DateTime.UtcNow;
    }
}
