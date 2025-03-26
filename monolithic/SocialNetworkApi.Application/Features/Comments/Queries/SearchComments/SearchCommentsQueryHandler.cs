using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Comments.Queries;

public class SearchCommentsQueryHandler : IRequestHandler<SearchCommentsQuery, PagedResultDto<CommentDto>>
{
    private readonly IRepository<CommentEntity> _commentRepository;
    private readonly IMapper _mapper;

    public SearchCommentsQueryHandler(IRepository<CommentEntity> commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<CommentDto>> Handle(SearchCommentsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var searchQuery = _commentRepository.GetAll().Where(c => c.PostId == request.PostId);

        var totalCount = await searchQuery.CountAsync(cancellationToken);

        var listComments = await searchQuery
            .Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize)
            .OrderByDescending(c => c.CreatedAt)
            .Include(c => c.User)
            .ToListAsync(cancellationToken);

        
        var listCommentDtos = listComments.Select(_mapper.Map<CommentDto>);

        return PagedResultDto<CommentDto>.Success(listCommentDtos).WithPage(pagedRequest.PageIndex, totalCount);
    }
}
