using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>
/// Query to retrieve a specific post by ID.
/// </summary>
public record GetPostByIdQuery(string Id, string? UserId = null) : IRequest<PostDto?>;

/// <summary>
/// Handler for the GetPostByIdQuery.
/// </summary>
public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    private readonly IPostRepository _postRepository;
    private readonly ILikeRepository _likeRepository;

    public GetPostByIdQueryHandler(IPostRepository postRepository, ILikeRepository likeRepository)
    {
        _postRepository = postRepository;
        _likeRepository = likeRepository;
    }

    public async Task<PostDto?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);

        if (post is null)
        {
            return null;
        }

        var dto = CreatePostCommandHandler.MapToDto(post);
        
        // Populate UserHasLiked if userId is provided
        if (!string.IsNullOrEmpty(request.UserId))
        {
            var userHasLiked = await _likeRepository.ExistsAsync(post.Id, request.UserId, cancellationToken);
            dto = dto with { UserHasLiked = userHasLiked };
        }

        return dto;
    }
}
