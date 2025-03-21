using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class DeleteChatMessageCommandHandler : IRequestHandler<DeleteChatMessageCommand, CommandResultDto<Guid>>
{
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;

    public DeleteChatMessageCommandHandler(IRepository<ChatMessageEntity> chatMessageRepository)
    {
        _chatMessageRepository = chatMessageRepository;
    }

    public async Task<CommandResultDto<Guid>> Handle(DeleteChatMessageCommand request, CancellationToken cancellationToken)
    {
        var chatMessage = await _chatMessageRepository.GetByIdAsync(request.Id);
        if (chatMessage == null)
        {
            return CommandResultDto<Guid>.Failure("Chat message not found.");
        }

        await _chatMessageRepository.DeleteAsync(chatMessage);
        return CommandResultDto<Guid>.Success(chatMessage.Id);
    }
}
