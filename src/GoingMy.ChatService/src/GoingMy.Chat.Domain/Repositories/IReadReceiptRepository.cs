namespace GoingMy.Chat.Domain.Repositories;

/// <summary>
/// Interface for ReadReceipt repository operations.
/// </summary>
public interface IReadReceiptRepository
{
    /// <summary>
    /// Gets all read receipts for a conversation.
    /// </summary>
    Task<IEnumerable<ReadReceipt>> GetByConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets read receipts for a specific message.
    /// </summary>
    Task<IEnumerable<ReadReceipt>> GetByMessageAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts a read receipt (idempotent per user+message pair).
    /// </summary>
    Task UpsertAsync(ReadReceipt receipt, CancellationToken cancellationToken = default);
}
