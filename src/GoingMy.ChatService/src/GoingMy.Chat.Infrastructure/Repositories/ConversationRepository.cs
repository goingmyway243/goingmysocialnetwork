using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using GoingMy.Chat.Infrastructure.Data;
using MongoDB.Driver;

namespace GoingMy.Chat.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of the Conversation repository.
/// </summary>
public class ConversationRepository(MongoDbContext context) : IConversationRepository
{
    public async Task<IEnumerable<Conversation>> GetByParticipantAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await context.Conversations
            .Find(c => c.ParticipantIds.Contains(userId))
            .SortByDescending(c => c.LastMessageAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Conversations
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Conversation?> GetByParticipantsAsync(string userIdA, string userIdB, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.AnyEq(c => c.ParticipantIds, userIdA),
            Builders<Conversation>.Filter.AnyEq(c => c.ParticipantIds, userIdB),
            Builders<Conversation>.Filter.Size(c => c.ParticipantIds, 2)
        );
        return await context.Conversations.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await context.Conversations.InsertOneAsync(conversation, cancellationToken: cancellationToken);
        return conversation;
    }

    public async Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.Eq(c => c.Id, conversation.Id);
        var result = await context.Conversations.ReplaceOneAsync(filter, conversation, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"Conversation {conversation.Id} not found.");

        return conversation;
    }
}
