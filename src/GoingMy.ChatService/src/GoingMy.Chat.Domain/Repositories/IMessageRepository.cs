namespace GoingMy.Chat.Domain.Repositories;

/// <summary>
/// Interface for Message repository operations.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Retrieves all messages for a conversation, ordered oldest-first.
    /// </summary>
    Task<IEnumerable<Message>> GetByConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves paginated messages for a conversation, ordered newest-first (for infinite scroll).
    /// </summary>
    Task<(IEnumerable<Message> Messages, bool HasMore)> GetPagedByConversationAsync(string conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches messages by content within a conversation.
    /// </summary>
    Task<IEnumerable<Message>> SearchAsync(string conversationId, string searchTerm, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a message by ID.
    /// </summary>
    Task<Message?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new message.
    /// </summary>
    Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing message (e.g. soft-delete, edit).
    /// </summary>
    Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default);
}
