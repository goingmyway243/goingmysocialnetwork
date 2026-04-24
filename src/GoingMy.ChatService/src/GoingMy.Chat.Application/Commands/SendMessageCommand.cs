using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using MediatR;
using MongoDB.Bson;

namespace GoingMy.Chat.Application.Commands;

/// <summary>
/// Command to send a message within an existing conversation.
/// </summary>
public record SendMessageCommand(
    string ConversationId,
    string SenderId,
    string SenderUsername,
    string Content
) : IRequest<MessageDto>;

/// <summary>
/// Handler for the SendMessageCommand.
/// </summary>
public class SendMessageCommandHandler(
    IMessageRepository messageRepository,
    IConversationRepository conversationRepository)
    : IRequestHandler<SendMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found.");

        if (!conversation.ParticipantIds.Contains(request.SenderId))
            throw new UnauthorizedAccessException("Sender is not a participant of this conversation.");

        var message = new Message(
            id: ObjectId.GenerateNewId().ToString(),
            conversationId: request.ConversationId,
            senderId: request.SenderId,
            senderUsername: request.SenderUsername,
            content: request.Content,
            sentAt: DateTime.UtcNow
        );

        var saved = await messageRepository.AddAsync(message, cancellationToken);

        conversation.UpdateLastMessage(request.Content.Length > 80
            ? request.Content[..80] + "…"
            : request.Content);
        await conversationRepository.UpdateAsync(conversation, cancellationToken);

        return new MessageDto(
            saved.Id,
            saved.ConversationId,
            saved.SenderId,
            saved.SenderUsername,
            saved.Content,
            saved.SentAt,
            saved.IsDeleted,
            saved.EditedContent,
            saved.EditedAt
        );
    }
}
