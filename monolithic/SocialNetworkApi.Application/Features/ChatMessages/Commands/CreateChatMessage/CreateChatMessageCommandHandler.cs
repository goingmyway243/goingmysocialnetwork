using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class CreateChatMessageCommandHandler : IRequestHandler<CreateChatMessageCommand, CommandResultDto<ChatMessageDto>>
{
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;
    private readonly IMapper _mapper;

    public CreateChatMessageCommandHandler(
        IRepository<ChatMessageEntity> chatMessageRepository, 
        IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<ChatMessageDto>> Handle(CreateChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return CommandResultDto<ChatMessageDto>.Failure("Message is required!");
        }

        if (request.UserId == default ||  request.ChatroomId == default)
        {
            return CommandResultDto<ChatMessageDto>.Failure("Invalid user or chatroom!");
        }

        var chatMessage = _mapper.Map<ChatMessageEntity>(request);
        chatMessage.Id = Guid.NewGuid();


        await _chatMessageRepository.InsertAsync(chatMessage);
        return CommandResultDto<ChatMessageDto>.Success(_mapper.Map<ChatMessageDto>(chatMessage));
    }
}
