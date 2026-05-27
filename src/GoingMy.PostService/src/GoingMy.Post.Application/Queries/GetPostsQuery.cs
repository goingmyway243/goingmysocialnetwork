using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>
/// Response DTO for paginated posts.
/// </summary>
public record GetPostsResponseDto(IEnumerable<PostDto> Posts, bool HasMore);

/// <summary>
/// Query to retrieve paginated posts.
/// </summary>
public record GetPostsQuery(
    string? UserId = null,
    int PageNumber = 0,
    int PageSize = 20
) : IRequest<GetPostsResponseDto>;

/// <summary>
/// Handler for the GetPostsQuery with pagination support.
/// </summary>
public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, GetPostsResponseDto>
{
    private readonly IPostRepository _postRepository;
    private readonly ILikeRepository _likeRepository;

    public GetPostsQueryHandler(IPostRepository postRepository, ILikeRepository likeRepository)
    {
        _postRepository = postRepository;
        _likeRepository = likeRepository;
    }

    public async Task<GetPostsResponseDto> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetAllAsync(cancellationToken);
        
        // Calculate pagination
        var totalCount = posts.Count();
        var skip = request.PageNumber * request.PageSize;
        var take = request.PageSize;
        var hasMore = skip + take < totalCount;
        
        // Apply pagination and mapping
        var dtos = posts
            .OrderByDescending(p => p.CreatedAt) // Sort by creation date, newest first
            .Skip(skip)
            .Take(take)
            .Select(CreatePostCommandHandler.MapToDto)
            .ToList();

        // Populate UserHasLiked in one query to avoid N+1 repository calls
        if (!string.IsNullOrEmpty(request.UserId))
        {
            var likedPostIds = await _likeRepository.GetLikedPostIdsAsync(
                request.UserId,
                dtos.Select(dto => dto.Id),
                cancellationToken);

            var result = dtos
                .Select(dto => dto with { UserHasLiked = likedPostIds.Contains(dto.Id) })
                .ToList();

            return new GetPostsResponseDto(result, hasMore);
        }

        return new GetPostsResponseDto(dtos, hasMore);
    }
}
