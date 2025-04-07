using AutoMapper;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Posts.Queries;

public class SearchPostsQueryHandler : IRequestHandler<SearchPostsQuery, PagedResultDto<PostDto>>
{
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IRepository<LikeEntity> _likeRepository;
    private readonly IMapper _mapper;

    public SearchPostsQueryHandler(
        IRepository<PostEntity> postRepository,
        IRepository<UserEntity> userRepository,
        IRepository<LikeEntity> likeRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _likeRepository = likeRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<PostDto>> Handle(SearchPostsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequest = request.PagedRequest;
        var builder = Builders<PostEntity>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(request.SearchText))
        {
            filter &= builder.Regex(p => p.Caption, new BsonRegularExpression(request.SearchText, "i"));
        }

        if (request.OwnerId != null)
        {
            filter &= builder.Eq(p => p.UserId, request.OwnerId);
        }

        var totalCount = await _postRepository.GetAll().CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var posts = await _postRepository.GetAll()
                    .Aggregate()
                    .Match(filter)
                    .Project(p => new
                    {
                        Document = p,
                        SortDate = p.ModifiedAt ?? p.CreatedAt
                    })
                    .SortByDescending(p => p.SortDate)
                    .Project(p => p.Document)
                    .Skip(pagedRequest.SkipCount)
                    .Limit(pagedRequest.PageSize)
                    .ToListAsync(cancellationToken);

        var userIds = posts.Select(p => p.UserId).Distinct().ToList();
        var distincUsers = await _userRepository.FindAsync(u => userIds.Contains(u.Id));

        var existingLikeByUser = await _likeRepository.FindAsync(l => l.UserId == request.CurrentUserId && posts.Select(p => p.Id).Contains(l.PostId));

        var result = posts.Select(_mapper.Map<PostDto>).ToList();
        result.ForEach(p => {
            p.IsLikedByUser = existingLikeByUser.FirstOrDefault(l => l.PostId == p.Id) != null;

            var userInfo = distincUsers.FirstOrDefault(u => u.Id == p.UserId);
            if (userInfo != null)
            {
                p.User = _mapper.Map<UserDto>(userInfo);
            }
        });

        return PagedResultDto<PostDto>.Success(result)
            .WithPage(pagedRequest.PageIndex, (int)totalCount);
    }
}
