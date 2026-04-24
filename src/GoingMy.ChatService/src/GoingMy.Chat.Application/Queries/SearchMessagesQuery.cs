using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain.Repositories;
using MediatR;

namespace GoingMy.Chat.Application.Queries;

/// <summary>
/// Query to search messages within a conversation by content.
/// </summary>
public record SearchMessagesQuery(
    string ConversationId,
    string RequestingUserId,
    string SearchTerm,
    int Limit = 20
) : IRequest<IEnumerable<MessageDto>>;

/// <summary>
/// Handler for SearchMessagesQuery.
/// </summary>
public class SearchMessagesQueryHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository)
    : IRequestHandler<SearchMessagesQuery, IEnumerable<MessageDto>>
{
    public async Task<IEnumerable<MessageDto>> Handle(SearchMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found.");

        if (!conversation.ParticipantIds.Contains(request.RequestingUserId))
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        var messages = await messageRepository.SearchAsync(
            request.ConversationId, request.SearchTerm, request.Limit, cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.ConversationId,
            m.SenderId,
            m.SenderUsername,
            m.Content,
            m.SentAt,
            m.IsDeleted,
            m.EditedContent,
            m.EditedAt
        ));
    }
}
