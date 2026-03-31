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
    /// Retrieves a message by ID.
    /// </summary>
    Task<Message?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new message.
    /// </summary>
    Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing message (e.g. soft-delete).
    /// </summary>
    Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default);
}
