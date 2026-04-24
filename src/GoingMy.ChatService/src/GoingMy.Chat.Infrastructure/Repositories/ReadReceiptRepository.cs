using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using GoingMy.Chat.Infrastructure.Data;
using MongoDB.Driver;

namespace GoingMy.Chat.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of the ReadReceipt repository.
/// </summary>
public class ReadReceiptRepository(MongoDbContext context) : IReadReceiptRepository
{
    public async Task<IEnumerable<ReadReceipt>> GetByConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        return await context.ReadReceipts
            .Find(r => r.ConversationId == conversationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReadReceipt>> GetByMessageAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return await context.ReadReceipts
            .Find(r => r.MessageId == messageId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(ReadReceipt receipt, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ReadReceipt>.Filter.And(
            Builders<ReadReceipt>.Filter.Eq(r => r.MessageId, receipt.MessageId),
            Builders<ReadReceipt>.Filter.Eq(r => r.ReadByUserId, receipt.ReadByUserId)
        );

        var options = new ReplaceOptions { IsUpsert = true };
        await context.ReadReceipts.ReplaceOneAsync(filter, receipt, options, cancellationToken);
    }
}
