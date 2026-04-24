using GoingMy.Chat.Domain.Repositories;
using MediatR;

namespace GoingMy.Chat.Application.Commands;

/// <summary>
/// Command to soft-delete a message. Only the original sender may delete.
/// </summary>
public record DeleteMessageCommand(
    string ConversationId,
    string MessageId,
    string RequestingUserId
) : IRequest;

/// <summary>
/// Handler for DeleteMessageCommand.
/// </summary>
public class DeleteMessageCommandHandler(
    IMessageRepository messageRepository,
    IConversationRepository conversationRepository)
    : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found.");

        if (!conversation.ParticipantIds.Contains(request.RequestingUserId))
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        var message = await messageRepository.GetByIdAsync(request.MessageId, cancellationToken)
            ?? throw new InvalidOperationException($"Message {request.MessageId} not found.");

        if (message.SenderId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the original sender can delete this message.");

        message.Delete();
        await messageRepository.UpdateAsync(message, cancellationToken);
    }
}
