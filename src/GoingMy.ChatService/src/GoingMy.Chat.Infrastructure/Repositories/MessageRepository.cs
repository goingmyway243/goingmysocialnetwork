using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using GoingMy.Chat.Infrastructure.Data;
using MongoDB.Driver;

namespace GoingMy.Chat.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of the Message repository.
/// </summary>
public class MessageRepository(MongoDbContext context) : IMessageRepository
{
    public async Task<IEnumerable<Message>> GetByConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        return await context.Messages
            .Find(m => m.ConversationId == conversationId)
            .SortBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Message?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Messages
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await context.Messages.InsertOneAsync(message, cancellationToken: cancellationToken);
        return message;
    }

    public async Task<Message> UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Message>.Filter.Eq(m => m.Id, message.Id);
        var result = await context.Messages.ReplaceOneAsync(filter, message, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"Message {message.Id} not found.");

        return message;
    }
}
