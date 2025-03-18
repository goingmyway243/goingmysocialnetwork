using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class UpdateChatMessageCommandHandler : IRequestHandler<UpdateChatMessageCommand, CommandResult<ChatMessageDto>>
{
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;
    private readonly IMapper _mapper;

    public UpdateChatMessageCommandHandler(
        IRepository<ChatMessageEntity> chatMessageRepository, 
        IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _mapper = mapper;
    }

    public async Task<CommandResult<ChatMessageDto>> Handle(UpdateChatMessageCommand request, CancellationToken cancellationToken)
    {
        var chatMessage = await _chatMessageRepository.GetByIdAsync(request.Id);
        if (chatMessage == null)
        {
            return CommandResult<ChatMessageDto>.Failure("Chat message not found.");
        }

        chatMessage.Message = request.Message;

        await _chatMessageRepository.UpdateAsync(chatMessage);
        return CommandResult<ChatMessageDto>.Success(_mapper.Map<ChatMessageDto>(chatMessage));
    }
}
