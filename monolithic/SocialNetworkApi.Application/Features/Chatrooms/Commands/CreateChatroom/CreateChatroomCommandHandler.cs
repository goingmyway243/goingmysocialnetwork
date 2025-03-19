using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class CreateChatroomCommandHandler : IRequestHandler<CreateChatroomCommand, CommandResult<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public CreateChatroomCommandHandler(
        IRepository<ChatroomEntity> chatroomRepository,
        IRepository<UserEntity> userRepository,
        IMapper mapper)
    {
        _chatroomRepository = chatroomRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CommandResult<ChatroomDto>> Handle(CreateChatroomCommand request, CancellationToken cancellationToken)
    {
        if (request.ParticipantIds.Count == 0)
        {
            return CommandResult<ChatroomDto>.Failure("Cannot create a chatroom without participant.");
        }

        var chatroom = new ChatroomEntity
        {
            Id = Guid.NewGuid(),
            ChatroomName = request.ChatroomName,
            Participants = request.ParticipantIds.Select(p => new ChatroomParticipantEntity { UserId = p }).ToList()
        };

        await _chatroomRepository.InsertAsync(chatroom);

        var resultDto = new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName
        };

        return CommandResult<ChatroomDto>.Success(resultDto);
    }
}
