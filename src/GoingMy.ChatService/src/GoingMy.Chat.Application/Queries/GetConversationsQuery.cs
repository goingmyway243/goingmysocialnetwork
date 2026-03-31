using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain.Repositories;
using MediatR;

namespace GoingMy.Chat.Application.Queries;

/// <summary>
/// Query to retrieve all conversations for a user, ordered by most recent activity.
/// </summary>
public record GetConversationsQuery(string UserId) : IRequest<IEnumerable<ConversationDto>>;

/// <summary>
/// Handler for the GetConversationsQuery.
/// </summary>
public class GetConversationsQueryHandler(IConversationRepository conversationRepository)
    : IRequestHandler<GetConversationsQuery, IEnumerable<ConversationDto>>
{
    public async Task<IEnumerable<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await conversationRepository.GetByParticipantAsync(request.UserId, cancellationToken);

        return conversations
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => new ConversationDto(
                c.Id,
                c.ParticipantIds,
                c.ParticipantUsernames,
                c.CreatedAt,
                c.LastMessageAt,
                c.LastMessagePreview
            ));
    }
}
