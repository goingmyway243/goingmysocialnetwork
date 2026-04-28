using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>
/// Query to retrieve all posts.
/// </summary>
public record GetPostsQuery(string? UserId = null) : IRequest<IEnumerable<PostDto>>;

/// <summary>
/// Handler for the GetPostsQuery.
/// </summary>
public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, IEnumerable<PostDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly ILikeRepository _likeRepository;

    public GetPostsQueryHandler(IPostRepository postRepository, ILikeRepository likeRepository)
    {
        _postRepository = postRepository;
        _likeRepository = likeRepository;
    }

    public async Task<IEnumerable<PostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetAllAsync(cancellationToken);
        var dtos = posts.Select(CreatePostCommandHandler.MapToDto).ToList();

        // Populate UserHasLiked for each post if userId is provided
        if (!string.IsNullOrEmpty(request.UserId))
        {
            var result = new List<PostDto>();
            foreach (var dto in dtos)
            {
                var userHasLiked = await _likeRepository.ExistsAsync(dto.Id, request.UserId, cancellationToken);
                result.Add(dto with { UserHasLiked = userHasLiked });
            }
            return result;
        }

        return dtos;
    }
}
