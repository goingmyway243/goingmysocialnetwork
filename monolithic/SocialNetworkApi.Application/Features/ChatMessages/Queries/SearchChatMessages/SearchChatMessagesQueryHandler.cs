using AutoMapper;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.ChatMessages.Queries;

public class SearchChatMessagesQueryHandler : IRequestHandler<SearchChatMessagesQuery, PagedResultDto<ChatMessageDto>>
{
    private readonly IRepository<ChatMessageEntity> _chatMessageRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public SearchChatMessagesQueryHandler(
        IRepository<ChatMessageEntity> chatMessageRepository,
        IRepository<UserEntity> userRepository,
        IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ChatMessageDto>> Handle(SearchChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var builder = Builders<ChatMessageEntity>.Filter;
        var filter = builder.Eq(m => m.ChatroomId, request.ChatroomId);

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            filter &= builder.Regex(m => m.Message, new BsonRegularExpression(request.SearchText, "i"));
        }

        var totalCount = await _chatMessageRepository.GetAll().CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var messages = await _chatMessageRepository.GetAll()
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip(pagedRequest.SkipCount)
            .Limit(pagedRequest.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = messages.Select(m => m.UserId).Distinct().ToList();
        var users = await _userRepository.FindAsync(u => userIds.Contains(u.Id));

        var result = messages.Select(_mapper.Map<ChatMessageDto>).ToList();
        result.ForEach(messageDto =>
        {
            var user = users.FirstOrDefault(u => u.Id == messageDto.UserId);
            if (user != null)
            {
                messageDto.User = _mapper.Map<UserDto>(messageDto.User);
            }
        });

        return PagedResultDto<ChatMessageDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, (int)totalCount);
    }
}
