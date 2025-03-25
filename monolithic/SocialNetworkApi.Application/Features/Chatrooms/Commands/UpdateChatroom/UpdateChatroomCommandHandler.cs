using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class UpdateChatroomCommandHandler : IRequestHandler<UpdateChatroomCommand, CommandResultDto<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IMapper _mapper;

    public UpdateChatroomCommandHandler(IRepository<ChatroomEntity> chatroomRepository, IMapper mapper)
    {
        _chatroomRepository = chatroomRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<ChatroomDto>> Handle(UpdateChatroomCommand request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository
            .GetAll()
            .Where(cr => cr.Id == request.Id)
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cancellationToken);
        if (chatroom == null)
        {
            return CommandResultDto<ChatroomDto>.Failure("Chatroom not found.");
        }

        chatroom.ChatroomName = request.ChatroomName;

        await _chatroomRepository.UpdateAsync(chatroom);

        return CommandResultDto<ChatroomDto>.Success(new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName,
            Participants = chatroom.Participants.Select(u => _mapper.Map<UserDto>(u)).ToList()
        });
    }
}
