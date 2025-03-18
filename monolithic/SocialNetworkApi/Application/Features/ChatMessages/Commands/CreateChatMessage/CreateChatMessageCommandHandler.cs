using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class CreateChatMessageCommandHandler : IRequestHandler<CreateChatMessageCommand, CommandResult<ChatMessageDto>>
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

    public async Task<CommandResult<ChatMessageDto>> Handle(CreateChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return CommandResult<ChatMessageDto>.Failure("Message is required!");
        }

        if (request.UserId == default ||  request.ChatroomId == default)
        {
            return CommandResult<ChatMessageDto>.Failure("Invalid user or chatroom!");
        }

        var chatMessage = _mapper.Map<ChatMessageEntity>(request);
        chatMessage.Id = Guid.NewGuid();


        await _chatMessageRepository.InsertAsync(chatMessage);
        return CommandResult<ChatMessageDto>.Success(_mapper.Map<ChatMessageDto>(chatMessage));
    }
}
