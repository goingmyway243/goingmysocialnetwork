using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>
/// Query to retrieve a specific post by ID.
/// </summary>
public record GetPostByIdQuery(string Id) : IRequest<PostDto?>;

/// <summary>
/// Handler for the GetPostByIdQuery.
/// </summary>
public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostDto?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);

        if (post is null)
        {
            return null;
        }

        return new PostDto(
            Id: post.Id,
            Title: post.Title,
            Content: post.Content,
            UserId: post.UserId,
            Username: post.Username,
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt
        );
    }
}
