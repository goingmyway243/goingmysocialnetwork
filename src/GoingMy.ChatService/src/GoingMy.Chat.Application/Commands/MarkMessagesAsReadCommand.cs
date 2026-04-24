using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using MediatR;
using MongoDB.Bson;

namespace GoingMy.Chat.Application.Commands;

/// <summary>
/// Command to mark all unread messages in a conversation as read by the requesting user.
/// </summary>
public record MarkMessagesAsReadCommand(
    string ConversationId,
    string RequestingUserId,
    string RequestingUsername
) : IRequest<IEnumerable<ReadReceiptDto>>;

/// <summary>
/// Handler for MarkMessagesAsReadCommand.
/// </summary>
public class MarkMessagesAsReadCommandHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IReadReceiptRepository readReceiptRepository)
    : IRequestHandler<MarkMessagesAsReadCommand, IEnumerable<ReadReceiptDto>>
{
    public async Task<IEnumerable<ReadReceiptDto>> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found.");

        if (!conversation.ParticipantIds.Contains(request.RequestingUserId))
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        var messages = await messageRepository.GetByConversationAsync(request.ConversationId, cancellationToken);

        var now = DateTime.UtcNow;
        var receipts = new List<ReadReceiptDto>();

        foreach (var message in messages.Where(m => !m.IsDeleted && m.SenderId != request.RequestingUserId))
        {
            var receipt = new ReadReceipt(
                id: ObjectId.GenerateNewId().ToString(),
                conversationId: request.ConversationId,
                messageId: message.Id,
                readByUserId: request.RequestingUserId,
                readByUsername: request.RequestingUsername,
                readAt: now
            );

            await readReceiptRepository.UpsertAsync(receipt, cancellationToken);

            receipts.Add(new ReadReceiptDto(
                receipt.MessageId,
                receipt.ReadByUserId,
                receipt.ReadByUsername,
                receipt.ReadAt
            ));
        }

        return receipts;
    }
}
