using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

public record GetPostLikesQuery(string PostId) : IRequest<IEnumerable<LikeDto>>;

public class GetPostLikesQueryHandler(ILikeRepository likeRepository)
    : IRequestHandler<GetPostLikesQuery, IEnumerable<LikeDto>>
{
    public async Task<IEnumerable<LikeDto>> Handle(GetPostLikesQuery request, CancellationToken cancellationToken)
    {
        var likes = await likeRepository.GetByPostIdAsync(request.PostId, cancellationToken);
        return likes.Select(l => new LikeDto(l.Id, l.PostId, l.UserId, l.Username, l.CreatedAt));
    }
}
