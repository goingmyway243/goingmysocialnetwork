using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class CreateChatMessageCommandHandler : IRequestHandler<CreateChatMessageCommand, CommandResultDto<ChatMessageDto>>
{
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IMapper _mapper;

    public CreateChatMessageCommandHandler(
        IRepository<ChatMessageEntity> chatMessageRepository,
        IRepository<UserEntity> userRepository,
        IRepository<ChatroomEntity> chatroomRepository,
        IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _userRepository = userRepository;
        _chatroomRepository = chatroomRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<ChatMessageDto>> Handle(CreateChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return CommandResultDto<ChatMessageDto>.Failure("Message is required!");
        }

        var existingUser = _userRepository.GetByIdAsync(request.UserId);
        var existingChatroom = _chatroomRepository.GetByIdAsync(request.ChatroomId);
        if (existingUser == null || existingChatroom == null)
        {
            return CommandResultDto<ChatMessageDto>.Failure("Invalid user or chatroom!");
        }

        var chatMessage = _mapper.Map<ChatMessageEntity>(request);
        chatMessage.Id = Guid.NewGuid();

        await _chatMessageRepository.InsertAsync(chatMessage);
        return CommandResultDto<ChatMessageDto>.Success(_mapper.Map<ChatMessageDto>(chatMessage));
    }
}
