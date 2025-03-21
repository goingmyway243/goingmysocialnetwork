using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class UpdateChatMessageCommandHandler : IRequestHandler<UpdateChatMessageCommand, CommandResultDto<ChatMessageDto>>
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

    public async Task<CommandResultDto<ChatMessageDto>> Handle(UpdateChatMessageCommand request, CancellationToken cancellationToken)
    {
        var chatMessage = await _chatMessageRepository.GetByIdAsync(request.Id);
        if (chatMessage == null)
        {
            return CommandResultDto<ChatMessageDto>.Failure("Chat message not found.");
        }

        chatMessage.Message = request.Message;

        await _chatMessageRepository.UpdateAsync(chatMessage);
        return CommandResultDto<ChatMessageDto>.Success(_mapper.Map<ChatMessageDto>(chatMessage));
    }
}
