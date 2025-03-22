using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries;

public class SearchChatroomsQueryHandler : IRequestHandler<SearchChatroomsQuery, PagedResultDto<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public SearchChatroomsQueryHandler(
        IRepository<ChatroomEntity> chatroomRepository,
        IRepository<UserEntity> userRepository,
        IMapper mapper)
    {
        _chatroomRepository = chatroomRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ChatroomDto>> Handle(SearchChatroomsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var searchQuery = _chatroomRepository.GetAll()
            .AsNoTracking()
            .Where(cr => cr.Participants.Any(p => p.UserId == request.UserId));

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            searchQuery = searchQuery.Where(cr => EF.Functions.Like(cr.ChatroomName, $"%{request.SearchText}%"));
        }

        var totalCount = await searchQuery.CountAsync(cancellationToken);

        var chatrooms = await searchQuery
            .Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize)
            .Include(cr => cr.Participants)
            .ToListAsync(cancellationToken);
        if (chatrooms == null)
        {
            return PagedResultDto<ChatroomDto>.Failure("Unexpected error occured!")
                .WithPage(pagedRequest.PageIndex, totalCount);
        }

        var chatroomsParticipantIds = chatrooms
            .SelectMany(cr => cr.Participants.Select(p => p.UserId))
            .Distinct()
            .ToList();

        var distinctUsers = await _userRepository.GetAll()
            .Where(u => chatroomsParticipantIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var chatroomDtos = chatrooms.Select(chatroom => new ChatroomDto
        {
            Id = chatroom.Id,
            ChatroomName = chatroom.ChatroomName,
            Participants = chatroom.Participants
                .Where(p => distinctUsers.ContainsKey(p.UserId))
                .Select(p => _mapper.Map<UserDto>(distinctUsers[p.UserId]))
                .ToList()
        }).ToList();

        return PagedResultDto<ChatroomDto>.Success(chatroomDtos)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
