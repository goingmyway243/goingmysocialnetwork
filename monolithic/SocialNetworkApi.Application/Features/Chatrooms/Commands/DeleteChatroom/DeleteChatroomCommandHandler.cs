using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class DeleteChatroomCommandHandler : IRequestHandler<DeleteChatroomCommand, CommandResultDto<Guid>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;

    public DeleteChatroomCommandHandler(IRepository<ChatroomEntity> chatroomRepository)
    {
        _chatroomRepository = chatroomRepository;
    }

    public async Task<CommandResultDto<Guid>> Handle(DeleteChatroomCommand request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository.GetByIdAsync(request.Id);
        if (chatroom == null)
        {
            return CommandResultDto<Guid>.Failure("Chatroom not found.");
        }

        await _chatroomRepository.DeleteAsync(chatroom);
        return CommandResultDto<Guid>.Success(chatroom.Id);
    }
}
