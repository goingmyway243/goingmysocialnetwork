using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Infrastructure.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>
/// Query to retrieve all posts.
/// </summary>
public record GetPostsQuery() : IRequest<IEnumerable<PostDto>>;

/// <summary>
/// Handler for the GetPostsQuery.
/// </summary>
public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, IEnumerable<PostDto>>
{
    private readonly IPostRepository _postRepository;

    public GetPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IEnumerable<PostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetAllAsync(cancellationToken);

        return posts.Select(p => new PostDto(
            Id: p.Id,
            Title: p.Title,
            Content: p.Content,
            UserId: p.UserId,
            Username: p.Username,
            CreatedAt: p.CreatedAt,
            UpdatedAt: p.UpdatedAt
        ));
    }
}
