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
    private readonly IRepository<LikeEntity> _likeRepository;
    private readonly IMapper _mapper;

    public SearchPostsQueryHandler(
        IRepository<PostEntity> postRepository,
        IRepository<LikeEntity> likeRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _likeRepository = likeRepository;
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
            .Take(pagedRequest.PageSize);

        var existingLikeByUser = await query.Join(
                _likeRepository.GetAll().Where(l => l.UserId == request.CurrentUserId),
                p => p.Id,
                l => l.PostId,
                (posts, likes) => likes
            ).ToListAsync(cancellationToken);

        var posts = await query.Include(p => p.User)
            .Include(p => p.Contents)
            .ToListAsync(cancellationToken);

        var result = posts.Select(_mapper.Map<PostDto>).ToList();
        result.ForEach(p => p.IsLikedByUser = existingLikeByUser.FirstOrDefault(l => l.PostId == p.Id) != null);

        return PagedResultDto<PostDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, totalCount);
    }
}
