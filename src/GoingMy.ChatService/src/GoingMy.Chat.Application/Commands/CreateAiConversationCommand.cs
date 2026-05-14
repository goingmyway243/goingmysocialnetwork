using GoingMy.Chat.Application.Dtos;
using GoingMy.Chat.Domain;
using GoingMy.Chat.Domain.Repositories;
using GoingMy.Shared;
using MediatR;
using MongoDB.Bson;

namespace GoingMy.Chat.Application.Commands;

/// <summary>
/// Command to create (or retrieve) an AI assistant conversation for the given user.
/// Only one AI conversation per user is permitted; subsequent calls return the existing one.
/// </summary>
public record CreateAiConversationCommand(
    string UserId,
    string Username
) : IRequest<ConversationDto>;

public class CreateAiConversationCommandHandler(IConversationRepository conversationRepository)
    : IRequestHandler<CreateAiConversationCommand, ConversationDto>
{
    public async Task<ConversationDto> Handle(CreateAiConversationCommand request, CancellationToken cancellationToken)
    {
        var existing = await conversationRepository.GetByParticipantsAsync(
            request.UserId, SharedServices.AiAssistant, cancellationToken);

        if (existing is not null)
            return MapToDto(existing);

        var conversation = new Conversation(
            id: ObjectId.GenerateNewId().ToString(),
            participantIds: [request.UserId, SharedServices.AiAssistant],
            participantUsernames: [request.Username, SharedServices.AiAssistantUsername],
            createdAt: DateTime.UtcNow,
            isAiConversation: true
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
        c.LastMessagePreview,
        c.IsAiConversation
    );
}
