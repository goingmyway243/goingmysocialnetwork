using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class UpdateChatroomCommandHandler : IRequestHandler<UpdateChatroomCommand, CommandResultDto<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;

    public UpdateChatroomCommandHandler(IRepository<ChatroomEntity> chatroomRepository)
    {
        _chatroomRepository = chatroomRepository;
    }

    public async Task<CommandResultDto<ChatroomDto>> Handle(UpdateChatroomCommand request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository.GetByIdAsync(request.Id);
        if (chatroom == null)
        {
            return CommandResultDto<ChatroomDto>.Failure("Chatroom not found.");
        }

        chatroom.ChatroomName = request.ChatroomName;

        await _chatroomRepository.UpdateAsync(chatroom);

        return CommandResultDto<ChatroomDto>.Success(new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName
        });
    }
}
