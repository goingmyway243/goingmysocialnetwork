using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>
/// Query to retrieve a paginated list of posts authored by a specific user.
/// </summary>
public record GetUserPostsQuery(string UserId, int Page = 1, int PageSize = 20)
    : IRequest<IEnumerable<PostDto>>;

/// <summary>
/// Handler for the GetUserPostsQuery.
/// </summary>
public class GetUserPostsQueryHandler(IPostRepository postRepository)
    : IRequestHandler<GetUserPostsQuery, IEnumerable<PostDto>>
{
    public async Task<IEnumerable<PostDto>> Handle(GetUserPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await postRepository.GetByUserIdAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        return posts.Select(CreatePostCommandHandler.MapToDto);
    }
}
