namespace GoingMy.Chat.Domain.Repositories;

/// <summary>
/// Interface for Conversation repository operations.
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Retrieves all conversations for a given participant.
    /// </summary>
    Task<IEnumerable<Conversation>> GetByParticipantAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a conversation by ID.
    /// </summary>
    Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an existing conversation between exactly two participants, or returns null.
    /// </summary>
    Task<Conversation?> GetByParticipantsAsync(string userIdA, string userIdB, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new conversation.
    /// </summary>
    Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing conversation.
    /// </summary>
    Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
}
