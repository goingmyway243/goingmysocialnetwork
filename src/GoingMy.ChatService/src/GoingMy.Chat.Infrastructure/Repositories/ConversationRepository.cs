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

    public async Task<long> BulkUpdateParticipantUsernameAsync(
        string userId,
        string newUsername,
        CancellationToken cancellationToken = default)
    {
        // Find all conversations where the user is a participant,
        // then update only the matching entry in ParticipantUsernames using the positional operator.
        // Because ParticipantIds and ParticipantUsernames are parallel arrays indexed identically,
        // we load each affected conversation and update in-memory, then replace.
        // MongoDB's positional $ operator works on a single matched element.

        var conversations = await context.Conversations
            .Find(c => c.ParticipantIds.Contains(userId))
            .ToListAsync(cancellationToken);

        long modifiedCount = 0;

        foreach (var conversation in conversations)
        {
            var index = conversation.ParticipantIds.IndexOf(userId);
            if (index < 0 || index >= conversation.ParticipantUsernames.Count)
                continue;

            if (conversation.ParticipantUsernames[index] == newUsername)
                continue; // already up to date

            conversation.ParticipantUsernames[index] = newUsername;

            var filter = Builders<Conversation>.Filter.Eq(c => c.Id, conversation.Id);
            var update = Builders<Conversation>.Update
                .Set(c => c.ParticipantUsernames, conversation.ParticipantUsernames);

            await context.Conversations.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            modifiedCount++;
        }

        return modifiedCount;
    }

    public async Task<long> RemoveParticipantAsync(string userId, CancellationToken cancellationToken = default)
    {
        var conversations = await context.Conversations
            .Find(c => c.ParticipantIds.Contains(userId))
            .ToListAsync(cancellationToken);

        long modifiedCount = 0;

        foreach (var conversation in conversations)
        {
            var index = conversation.ParticipantIds.IndexOf(userId);
            if (index < 0)
                continue;

            conversation.ParticipantIds.RemoveAt(index);
            if (index < conversation.ParticipantUsernames.Count)
                conversation.ParticipantUsernames.RemoveAt(index);

            var filter = Builders<Conversation>.Filter.Eq(c => c.Id, conversation.Id);
            var update = Builders<Conversation>.Update
                .Set(c => c.ParticipantIds, conversation.ParticipantIds)
                .Set(c => c.ParticipantUsernames, conversation.ParticipantUsernames);

            await context.Conversations.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            modifiedCount++;
        }

        return modifiedCount;
    }
}
