using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoingMy.Chat.Domain;

/// <summary>
/// Represents a private conversation between two users.
/// </summary>
public class Conversation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
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

    /// <summary>
    /// True when one participant is the AI assistant.
    /// </summary>
    public bool IsAiConversation { get; set; }

    public Conversation(string id, List<string> participantIds, List<string> participantUsernames, DateTime createdAt, bool isAiConversation = false)
    {
        Id = id;
        ParticipantIds = participantIds;
        ParticipantUsernames = participantUsernames;
        CreatedAt = createdAt;
        IsAiConversation = isAiConversation;
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
