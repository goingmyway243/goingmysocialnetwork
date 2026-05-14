using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain.Repositories;
using GoingMy.Shared;
using MediatR;

namespace GoingMy.Chat.Application.Queries;

/// <summary>
/// Query to retrieve all AI assistant conversations for a user.
/// </summary>
public record GetAiConversationsQuery(string UserId) : IRequest<IEnumerable<ConversationDto>>;

public class GetAiConversationsQueryHandler(IConversationRepository conversationRepository)
    : IRequestHandler<GetAiConversationsQuery, IEnumerable<ConversationDto>>
{
    public async Task<IEnumerable<ConversationDto>> Handle(GetAiConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await conversationRepository.GetByParticipantAsync(request.UserId, cancellationToken);

        return conversations
            .Where(c => c.IsAiConversation)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => new ConversationDto(
                c.Id,
                c.ParticipantIds,
                c.ParticipantUsernames,
                c.CreatedAt,
                c.LastMessageAt,
                c.LastMessagePreview,
                c.IsAiConversation
            ));
    }
}
