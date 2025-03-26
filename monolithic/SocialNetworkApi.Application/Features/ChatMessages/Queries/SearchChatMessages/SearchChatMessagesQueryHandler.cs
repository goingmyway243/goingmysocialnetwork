using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var searchQuery = _chatMessageRepository.GetAll().Where(m => m.ChatroomId == request.ChatroomId);

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            searchQuery = searchQuery.Where(m => m.Message.Contains(request.SearchText));
        }

        var totalCount = await searchQuery.CountAsync(cancellationToken);

        var messages = await searchQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize)
            .Include(m => m.User)
            .ToListAsync(cancellationToken);

        var result = messages.Select(_mapper.Map<ChatMessageDto>).ToList();
        return PagedResultDto<ChatMessageDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
