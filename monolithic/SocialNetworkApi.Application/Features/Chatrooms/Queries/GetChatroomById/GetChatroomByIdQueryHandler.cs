using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries;

public class GetChatroomByIdQueryHandler : IRequestHandler<GetChatroomByIdQuery, QueryResultDto<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IMapper _mapper;

    public GetChatroomByIdQueryHandler(IRepository<ChatroomEntity> chatroomRepository, IMapper mapper)
    {
        _chatroomRepository = chatroomRepository;
        _mapper = mapper;
    }

    public async Task<QueryResultDto<ChatroomDto>> Handle(GetChatroomByIdQuery request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository
            .GetAll()
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cr => cr.Id == request.Id, cancellationToken);
        if (chatroom == null)
        {
            return QueryResultDto<ChatroomDto>.Failure("Chatroom not found.");
        }

        var result = new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName,
            Participants = chatroom.Participants.Select(u => _mapper.Map<UserDto>(u)).ToList(),
        };

        return QueryResultDto<ChatroomDto>.Success(result);
    }
}
