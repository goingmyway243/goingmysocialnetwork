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

        var aggreatedSearchQuery = searchQuery
            .Include(cr => cr.Participants)
            .Select(cr => new
            {
                Chatroom = cr,
                LatestMessage = cr.ChatMessages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()
            })
            .OrderByDescending(p => p.LatestMessage != null ? p.LatestMessage.CreatedAt : DateTime.UtcNow.AddYears(-39))
            .Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize);

        var chatrooms = await aggreatedSearchQuery.ToListAsync(cancellationToken);

        var chatroomParticipantIds = chatrooms
            .SelectMany(cr => cr.Chatroom.Participants.Select(p => p.UserId))
            .Distinct()
            .ToList();

        var distinctUsers = await _userRepository.GetAll()
            .Where(u => chatroomParticipantIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var chatroomDtos = chatrooms.Select(chatroom =>
            new ChatroomDto
            {
                Id = chatroom.Chatroom.Id,
                ChatroomName = chatroom.Chatroom.ChatroomName,
                Participants = chatroom.Chatroom.Participants
                    .Where(p => distinctUsers.ContainsKey(p.UserId))
                    .Select(p => _mapper.Map<UserDto>(distinctUsers[p.UserId]))
                    .ToList(),
                LatestMessage = _mapper.Map<ChatMessageDto>(chatroom.LatestMessage)
            }
        )
        .OrderByDescending(cr => cr?.LatestMessage?.CreatedAt)
        .ToList();

        return PagedResultDto<ChatroomDto>.Success(chatroomDtos)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
