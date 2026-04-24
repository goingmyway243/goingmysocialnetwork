using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain.Repositories;
using MediatR;

namespace GoingMy.Chat.Application.Queries;

/// <summary>
/// Query to retrieve all read receipts for a conversation.
/// </summary>
public record GetReadReceiptsQuery(
    string ConversationId,
    string RequestingUserId
) : IRequest<IEnumerable<ReadReceiptDto>>;

/// <summary>
/// Handler for GetReadReceiptsQuery.
/// </summary>
public class GetReadReceiptsQueryHandler(
    IConversationRepository conversationRepository,
    IReadReceiptRepository readReceiptRepository)
    : IRequestHandler<GetReadReceiptsQuery, IEnumerable<ReadReceiptDto>>
{
    public async Task<IEnumerable<ReadReceiptDto>> Handle(GetReadReceiptsQuery request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found.");

        if (!conversation.ParticipantIds.Contains(request.RequestingUserId))
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        var receipts = await readReceiptRepository.GetByConversationAsync(request.ConversationId, cancellationToken);

        return receipts.Select(r => new ReadReceiptDto(
            r.MessageId,
            r.ReadByUserId,
            r.ReadByUsername,
            r.ReadAt
        ));
    }
}
