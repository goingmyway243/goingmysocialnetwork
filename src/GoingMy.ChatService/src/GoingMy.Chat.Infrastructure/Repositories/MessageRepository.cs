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

    public async Task<(IEnumerable<Message> Messages, bool HasMore)> GetPagedByConversationAsync(
        string conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = pageNumber * pageSize;
        var fetchCount = pageSize + 1; // fetch one extra to detect hasMore

        var items = await context.Messages
            .Find(m => m.ConversationId == conversationId)
            .SortByDescending(m => m.SentAt)
            .Skip(skip)
            .Limit(fetchCount)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return (items, hasMore);
    }

    public async Task<IEnumerable<Message>> SearchAsync(
        string conversationId, string searchTerm, int limit, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
            Builders<Message>.Filter.Eq(m => m.IsDeleted, false),
            Builders<Message>.Filter.Regex(m => m.Content, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
        );

        return await context.Messages
            .Find(filter)
            .SortByDescending(m => m.SentAt)
            .Limit(limit)
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
