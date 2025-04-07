using AutoMapper;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries;

public class SearchChatroomsQueryHandler : IRequestHandler<SearchChatroomsQuery, PagedResultDto<ChatroomDto>>
{
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public SearchChatroomsQueryHandler(
        IRepository<ChatroomEntity> chatroomRepository,
        IRepository<ChatMessageEntity> chatMessageRepository,
        IRepository<UserEntity> userRepository,
        IMapper mapper)
    {
        _chatroomRepository = chatroomRepository;
        _chatMessageRepository = chatMessageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ChatroomDto>> Handle(SearchChatroomsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var builder = Builders<ChatroomEntity>.Filter;
        var filter = builder.ElemMatch(cr => cr.ParticipantIds, p => p == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            filter &= builder.Regex(cr => cr.ChatroomName, new BsonRegularExpression(request.SearchText, "i"));
        }

        var totalCount = await _chatroomRepository.GetAll().CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var chatrooms = await _chatroomRepository.GetAll()
            .Find(filter)
            .Skip(pagedRequest.SkipCount)
            .Limit(pagedRequest.PageSize)
            .ToListAsync(cancellationToken);

        var chatroomIds = chatrooms.Select(cr => cr.Id).ToList();

        var latestMessages = await _chatMessageRepository.GetAll()
            .Aggregate()
            .Match(m => chatroomIds.Contains(m.ChatroomId))
            .SortByDescending(m => m.CreatedAt)
            .Group(m => m.ChatroomId, g => g.First())
            .ToListAsync(cancellationToken);


        var chatroomParticipantIds = chatrooms
            .SelectMany(cr => cr.ParticipantIds)
            .Distinct()
            .ToList();

        var chatroomParticipants = await _userRepository.FindAsync(u => chatroomParticipantIds.Contains(u.Id));
        var distinctUsers = chatroomParticipants.ToDictionary(u => u.Id, u => u);

        var chatroomDtos = chatrooms.Select(chatroom =>
            new ChatroomDto
            {
                Id = chatroom.Id,
                ChatroomName = chatroom.ChatroomName,
                Participants = chatroom.ParticipantIds
                    .Where(p => distinctUsers.ContainsKey(p))
                    .Select(p => _mapper.Map<UserDto>(distinctUsers[p]))
                    .ToList(),
                LatestMessage = _mapper.Map<ChatMessageDto>(latestMessages.FirstOrDefault(m => m.ChatroomId == chatroom.Id))
            }
        )
        .OrderByDescending(cr => cr?.LatestMessage?.CreatedAt)
        .ToList();

        return PagedResultDto<ChatroomDto>.Success(chatroomDtos)
            .WithPage(pagedRequest.PageIndex, (int)totalCount);
    }
}
