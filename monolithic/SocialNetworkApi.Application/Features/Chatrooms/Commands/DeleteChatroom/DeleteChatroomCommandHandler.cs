using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands.DeleteChatroom;

public class DeleteChatroomCommandHandler : IRequestHandler<DeleteChatroomCommand, CommandResult<Guid>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;

    public DeleteChatroomCommandHandler(IRepository<ChatroomEntity> chatroomRepository)
    {
        _chatroomRepository = chatroomRepository;
    }

    public async Task<CommandResult<Guid>> Handle(DeleteChatroomCommand request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository
            .GetAll()
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cr => cr.Id == request.Id, cancellationToken);
        if (chatroom == null)
        {
            return CommandResult<Guid>.Failure("Chatroom not found.");
        }

        await _chatroomRepository.DeleteAsync(chatroom);
        return CommandResult<Guid>.Success(chatroom.Id);
    }
}
