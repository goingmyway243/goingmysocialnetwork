using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries.GetChatroomById;

public class GetChatroomByIdQueryHandler : IRequestHandler<GetChatroomByIdQuery, QueryResult<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;

    public GetChatroomByIdQueryHandler(IRepository<ChatroomEntity> chatroomRepository)
    {
        _chatroomRepository = chatroomRepository;
    }

    public async Task<QueryResult<ChatroomDto>> Handle(GetChatroomByIdQuery request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository
            .GetAll()
            .Include(cr => cr.Participants)
            .FirstOrDefaultAsync(cr => cr.Id == request.Id, cancellationToken);
        if (chatroom == null)
        {
            return QueryResult<ChatroomDto>.Failure("Chatroom not found.");
        }

        var result = new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName,
            ParticipantIds = chatroom.Participants.Select(x => x.Id).ToList(),
        };

        return QueryResult<ChatroomDto>.Success(result);
    }
}
