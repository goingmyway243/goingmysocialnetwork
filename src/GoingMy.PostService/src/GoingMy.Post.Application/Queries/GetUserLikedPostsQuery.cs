using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>
/// Query to retrieve a paginated list of posts liked by a specific user.
/// </summary>
public record GetUserLikedPostsQuery(string UserId, int Page = 1, int PageSize = 20)
    : IRequest<IEnumerable<PostDto>>;

/// <summary>
/// Handler for the GetUserLikedPostsQuery.
/// Fetches the user's likes, then resolves the corresponding posts.
/// </summary>
public class GetUserLikedPostsQueryHandler(ILikeRepository likeRepository, IPostRepository postRepository)
    : IRequestHandler<GetUserLikedPostsQuery, IEnumerable<PostDto>>
{
    public async Task<IEnumerable<PostDto>> Handle(GetUserLikedPostsQuery request, CancellationToken cancellationToken)
    {
        var likes = await likeRepository.GetByUserIdAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        var postIds = likes.Select(l => l.PostId);

        var posts = await postRepository.GetByIdsAsync(postIds, cancellationToken);

        // Preserve order from likes (most-recently-liked first)
        var orderedPostIds = likes.Select(l => l.PostId).ToList();
        var postsById = posts.ToDictionary(p => p.Id);
        var ordered = orderedPostIds
            .Where(id => postsById.ContainsKey(id))
            .Select(id => postsById[id]);

        return ordered.Select(CreatePostCommandHandler.MapToDto);
    }
}
