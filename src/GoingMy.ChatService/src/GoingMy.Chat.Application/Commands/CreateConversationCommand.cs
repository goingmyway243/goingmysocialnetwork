using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using MediatR;
using MongoDB.Bson;

namespace GoingMy.Chat.Application.Commands;

/// <summary>
/// Command to create a new conversation between two participants.
/// Returns an existing conversation if one already exists.
/// </summary>
public record CreateConversationCommand(
    string InitiatorId,
    string InitiatorUsername,
    string RecipientId,
    string RecipientUsername
) : IRequest<ConversationDto>;

/// <summary>
/// Handler for the CreateConversationCommand.
/// </summary>
public class CreateConversationCommandHandler(IConversationRepository conversationRepository)
    : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // Return existing conversation to avoid duplicates
        var existing = await conversationRepository.GetByParticipantsAsync(
            request.InitiatorId, request.RecipientId, cancellationToken);

        if (existing is not null)
            return MapToDto(existing);

        var conversation = new Conversation(
            id: ObjectId.GenerateNewId().ToString(),
            participantIds: [request.InitiatorId, request.RecipientId],
            participantUsernames: [request.InitiatorUsername, request.RecipientUsername],
            createdAt: DateTime.UtcNow
        );

        var created = await conversationRepository.AddAsync(conversation, cancellationToken);
        return MapToDto(created);
    }

    private static ConversationDto MapToDto(Conversation c) => new(
        c.Id,
        c.ParticipantIds,
        c.ParticipantUsernames,
        c.CreatedAt,
        c.LastMessageAt,
        c.LastMessagePreview
    );
}
