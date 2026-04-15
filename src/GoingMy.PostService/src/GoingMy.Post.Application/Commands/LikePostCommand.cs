using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Entities;
using GoingMy.Post.Domain.Repositories;
using MassTransit;
using MediatR;

namespace GoingMy.Post.Application.Commands;

public record LikePostCommand(string PostId, string UserId, string Username) : IRequest<LikeDto>;

public class LikePostCommandHandler(IPostRepository postRepository, ILikeRepository likeRepository)
    : IRequestHandler<LikePostCommand, LikeDto>
{
    public async Task<LikeDto> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        _ = await postRepository.GetByIdAsync(request.PostId, cancellationToken)
            ?? throw new InvalidOperationException($"Post '{request.PostId}' not found.");

        if (await likeRepository.ExistsAsync(request.PostId, request.UserId, cancellationToken))
            throw new InvalidOperationException("You have already liked this post.");

        var like = new Like(
            id: NewId.NextSequentialGuid().ToString(),
            postId: request.PostId,
            userId: request.UserId,
            username: request.Username,
            createdAt: DateTime.UtcNow);

        await likeRepository.AddAsync(like, cancellationToken);
        await postRepository.IncrementLikesAsync(request.PostId, cancellationToken);

        return new LikeDto(like.Id, like.PostId, like.UserId, like.Username, like.CreatedAt);
    }
}
