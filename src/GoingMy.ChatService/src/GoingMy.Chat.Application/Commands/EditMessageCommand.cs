using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain.Repositories;
using MediatR;

namespace GoingMy.Chat.Application.Commands;

/// <summary>
/// Command to edit an existing message's content. Only the original sender may edit.
/// </summary>
public record EditMessageCommand(
    string ConversationId,
    string MessageId,
    string NewContent,
    string RequestingUserId
) : IRequest<MessageDto>;

/// <summary>
/// Handler for EditMessageCommand.
/// </summary>
public class EditMessageCommandHandler(
    IMessageRepository messageRepository,
    IConversationRepository conversationRepository)
    : IRequestHandler<EditMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found.");

        if (!conversation.ParticipantIds.Contains(request.RequestingUserId))
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        var message = await messageRepository.GetByIdAsync(request.MessageId, cancellationToken)
            ?? throw new InvalidOperationException($"Message {request.MessageId} not found.");

        if (message.SenderId != request.RequestingUserId)
            throw new UnauthorizedAccessException("Only the original sender can edit this message.");

        if (message.IsDeleted)
            throw new InvalidOperationException("Cannot edit a deleted message.");

        message.Edit(request.NewContent);
        var updated = await messageRepository.UpdateAsync(message, cancellationToken);

        return new MessageDto(
            updated.Id,
            updated.ConversationId,
            updated.SenderId,
            updated.SenderUsername,
            updated.Content,
            updated.SentAt,
            updated.IsDeleted,
            updated.EditedContent,
            updated.EditedAt
        );
    }
}
