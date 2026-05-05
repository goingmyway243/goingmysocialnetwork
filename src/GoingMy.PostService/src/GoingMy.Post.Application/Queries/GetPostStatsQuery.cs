using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

/// <summary>Returns aggregate post statistics for admin reporting.</summary>
public record GetPostStatsQuery : IRequest<PostStatsDto>;

public class GetPostStatsQueryHandler : IRequestHandler<GetPostStatsQuery, PostStatsDto>
{
    private readonly IPostRepository _postRepository;

    public GetPostStatsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostStatsDto> Handle(GetPostStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _postRepository.GetStatsAsync(cancellationToken);
        return new PostStatsDto(
            stats.TotalPosts,
            stats.TotalLikes,
            stats.TotalComments,
            stats.PostsLast7Days,
            stats.PostsLast30Days);
    }
}
