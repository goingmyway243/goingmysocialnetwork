using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record UnfollowUserCommand(Guid FollowerId, Guid FolloweeId) : IRequest;

public class UnfollowUserCommandHandler(
    IUserFollowRepository userFollowRepository,
    IUserProfileRepository userProfileRepository)
    : IRequestHandler<UnfollowUserCommand>
{
    public async Task Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var isFollowing = await userFollowRepository.ExistsAsync(
            request.FollowerId, request.FolloweeId, cancellationToken);

        if (!isFollowing)
            throw new InvalidOperationException("Not following this user.");

        var follower = await userProfileRepository.GetByIdAsync(request.FollowerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Follower profile '{request.FollowerId}' not found.");

        var followee = await userProfileRepository.GetByIdAsync(request.FolloweeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Followee profile '{request.FolloweeId}' not found.");

        await userFollowRepository.DeleteAsync(request.FollowerId, request.FolloweeId, cancellationToken);

        follower.DecrementFollowing();
        followee.DecrementFollowers();

        await userProfileRepository.UpdateAsync(follower, cancellationToken);
        await userProfileRepository.UpdateAsync(followee, cancellationToken);
    }
}
