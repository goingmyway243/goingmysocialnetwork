using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Posts.Queries;

public class SearchPostsQueryHandler : IRequestHandler<SearchPostsQuery, PagedResultDto<PostDto>>
{
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IMapper _mapper;

    public SearchPostsQueryHandler(IRepository<PostEntity> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<PostDto>> Handle(SearchPostsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var query = _postRepository.GetAll();

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            query = query.Where(p => p.Caption.Contains(request.SearchText));
        }

        if (request.OwnerId != null)
        {
            query = query.Where(p => p.UserId == request.OwnerId);
        }

        var totalCount = await query.CountAsync();

        query = query.OrderByDescending(p => p.ModifiedAt ?? p.CreatedAt)
            .Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize)
            .Include(p => p.User)
            .Include(p => p.Contents);

        var posts = await query.ToListAsync(cancellationToken);

        var result = posts.Select(_mapper.Map<PostDto>);
        return PagedResultDto<PostDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
