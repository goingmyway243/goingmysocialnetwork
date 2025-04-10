using AutoMapper;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries;

internal class ChatroomWithMessages : ChatroomEntity
{
    public IEnumerable<ChatMessageEntity> ChatMessages { get; set; } = new List<ChatMessageEntity>();
}

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
            .Aggregate()
            .Match(filter)
            .Lookup<ChatroomEntity, ChatMessageEntity, ChatroomWithMessages>(
                _chatMessageRepository.GetAll(),
                cr => cr.Id,
                cm => cm.ChatroomId,
                cr => cr.ChatMessages
            )
            .Project(cr => new
            {
                Chatroom = new ChatroomEntity
                {
                    Id = cr.Id,
                    ChatroomName = cr.ChatroomName,
                    ParticipantIds = cr.ParticipantIds
                },
                LatestMessage = cr.ChatMessages.OrderByDescending(m => m.CreatedAt).FirstOrDefault(),
                SortDate = cr.ChatMessages.Any() ? cr.ChatMessages.Max(m => m.CreatedAt) : DateTime.UtcNow.AddYears(-39)
            })
            .SortByDescending(cr => cr.SortDate)
            .Skip(pagedRequest.SkipCount)
            .Limit(pagedRequest.PageSize)
            .ToListAsync(cancellationToken);

        var chatroomParticipantIds = chatrooms
            .SelectMany(cr => cr.Chatroom.ParticipantIds)
            .Distinct()
            .ToList();

        var chatroomParticipants = await _userRepository.FindAsync(u => chatroomParticipantIds.Contains(u.Id));
        var distinctUsers = chatroomParticipants.ToDictionary(u => u.Id, u => u);

        var chatroomDtos = chatrooms.Select(chatroom =>
            new ChatroomDto
            {
                Id = chatroom.Chatroom.Id,
                ChatroomName = chatroom.Chatroom.ChatroomName,
                Participants = chatroom.Chatroom.ParticipantIds
                    .Where(p => distinctUsers.ContainsKey(p))
                    .Select(p => _mapper.Map<UserDto>(distinctUsers[p]))
                    .ToList(),
                LatestMessage = _mapper.Map<ChatMessageDto>(chatroom.LatestMessage)
            }
        )
        .OrderByDescending(cr => cr?.LatestMessage?.CreatedAt)
        .ToList();

        return PagedResultDto<ChatroomDto>.Success(chatroomDtos)
            .WithPage(pagedRequest.PageIndex, (int)totalCount);
    }
}
