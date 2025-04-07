using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class CreateChatroomCommandHandler : IRequestHandler<CreateChatroomCommand, CommandResultDto<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;

    public CreateChatroomCommandHandler(
        IRepository<ChatroomEntity> chatroomRepository)
    {
        _chatroomRepository = chatroomRepository;
    }

    public async Task<CommandResultDto<ChatroomDto>> Handle(CreateChatroomCommand request, CancellationToken cancellationToken)
    {
        if (request.ParticipantIds.Count == 0)
        {
            return CommandResultDto<ChatroomDto>.Failure("Cannot create a chatroom without participant.");
        }

        var chatroom = new ChatroomEntity
        {
            Id = Guid.NewGuid(),
            ChatroomName = request.ChatroomName,
            ParticipantIds = request.ParticipantIds
        };

        await _chatroomRepository.InsertAsync(chatroom);

        var resultDto = new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName
        };

        return CommandResultDto<ChatroomDto>.Success(resultDto);
    }
}
