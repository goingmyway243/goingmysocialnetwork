using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries;

public class GetChatroomByIdQueryHandler : IRequestHandler<GetChatroomByIdQuery, QueryResultDto<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public GetChatroomByIdQueryHandler(IRepository<ChatroomEntity> chatroomRepository, IRepository<UserEntity> userRepository, IMapper mapper)
    {
        _chatroomRepository = chatroomRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<QueryResultDto<ChatroomDto>> Handle(GetChatroomByIdQuery request, CancellationToken cancellationToken)
    {
        var chatroom = await _chatroomRepository.GetByIdAsync(request.Id);
        if (chatroom == null)
        {
            return QueryResultDto<ChatroomDto>.Failure("Chatroom not found.");
        }

        var participants = await _userRepository.FindAsync(u => chatroom.ParticipantIds.Contains(u.Id));

        var result = new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName,
            Participants = participants.Select(_mapper.Map<UserDto>).ToList(),
        };

        return QueryResultDto<ChatroomDto>.Success(result);
    }
}
