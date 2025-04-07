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
    private readonly IMapper _mapper;

    public SearchChatMessagesQueryHandler(
        IRepository<ChatMessageEntity> chatMessageRepository,
        IMapper mapper)
    {
        _chatMessageRepository = chatMessageRepository;
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

        var result = messages.Select(_mapper.Map<ChatMessageDto>).ToList();
        return PagedResultDto<ChatMessageDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, (int)totalCount);
    }
}
