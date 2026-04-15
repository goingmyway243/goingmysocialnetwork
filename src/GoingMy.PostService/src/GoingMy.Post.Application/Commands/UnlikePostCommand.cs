using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Commands;

public record UnlikePostCommand(string PostId, string UserId) : IRequest<bool>;

public class UnlikePostCommandHandler(IPostRepository postRepository, ILikeRepository likeRepository)
    : IRequestHandler<UnlikePostCommand, bool>
{
    public async Task<bool> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
    {
        if (!await likeRepository.ExistsAsync(request.PostId, request.UserId, cancellationToken))
            throw new InvalidOperationException("You have not liked this post.");

        await likeRepository.DeleteAsync(request.PostId, request.UserId, cancellationToken);
        await postRepository.DecrementLikesAsync(request.PostId, cancellationToken);

        return true;
    }
}
