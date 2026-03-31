using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain.Repositories;
using MediatR;

namespace GoingMy.Chat.Application.Queries;

/// <summary>
/// Query to retrieve all messages in a conversation.
/// </summary>
public record GetConversationMessagesQuery(string ConversationId, string RequestingUserId) : IRequest<IEnumerable<MessageDto>>;

/// <summary>
/// Handler for the GetConversationMessagesQuery.
/// </summary>
public class GetConversationMessagesQueryHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository)
    : IRequestHandler<GetConversationMessagesQuery, IEnumerable<MessageDto>>
{
    public async Task<IEnumerable<MessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found.");

        if (!conversation.ParticipantIds.Contains(request.RequestingUserId))
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        var messages = await messageRepository.GetByConversationAsync(request.ConversationId, cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.ConversationId,
            m.SenderId,
            m.SenderUsername,
            m.Content,
            m.SentAt,
            m.IsDeleted
        ));
    }
}
