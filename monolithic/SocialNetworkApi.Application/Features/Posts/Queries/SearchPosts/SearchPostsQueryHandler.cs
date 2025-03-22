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

        query = query.Skip(pagedRequest.SkipCount)
            .Take(pagedRequest.PageSize)
            .OrderByDescending(p => p.ModifiedAt ?? p.CreatedAt)
            .Include(p => p.User)
            .Include(p => p.Contents);

        var result = await query.ToListAsync();
        if (result == null)
        {
            return PagedResultDto<PostDto>.Failure("Unexpected error occured!")
                .WithPage(pagedRequest.PageIndex, totalCount);
        }

        var items = result.Select(_mapper.Map<PostDto>);
        return PagedResultDto<PostDto>.Success(items)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
