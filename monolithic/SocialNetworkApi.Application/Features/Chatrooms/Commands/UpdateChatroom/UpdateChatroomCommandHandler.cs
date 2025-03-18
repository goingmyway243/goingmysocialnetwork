using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class UpdateChatroomCommandHandler : IRequestHandler<UpdateChatroomCommand, CommandResult<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;

    public UpdateChatroomCommandHandler(IRepository<ChatroomEntity> chatroomRepository)
    {
        _chatroomRepository = chatroomRepository;
    }

    public async Task<CommandResult<ChatroomDto>> Handle(UpdateChatroomCommand request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository
            .GetAll()
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cr => cr.Id == request.Id, cancellationToken);
        if (chatroom == null)
        {
            return CommandResult<ChatroomDto>.Failure("Chatroom not found.");
        }

        chatroom.ChatroomName = request.ChatroomName;

        await _chatroomRepository.UpdateAsync(chatroom);

        return CommandResult<ChatroomDto>.Success(new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName,
            ParticipantIds = chatroom.Participants.Select(x => x.Id).ToList()
        });
    }
}
