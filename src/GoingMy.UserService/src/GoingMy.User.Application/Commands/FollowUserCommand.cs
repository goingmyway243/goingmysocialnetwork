using GoingMy.Shared.Events;
using GoingMy.User.Domain.Entities;
using GoingMy.User.Domain.Repositories;
using MassTransit;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record FollowUserCommand(Guid FollowerId, Guid FolloweeId) : IRequest;

public class FollowUserCommandHandler(
    IUserFollowRepository userFollowRepository,
    IUserProfileRepository userProfileRepository,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<FollowUserCommand>
{
    public async Task Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        if (request.FollowerId == request.FolloweeId)
            throw new InvalidOperationException("A user cannot follow themselves.");

        var alreadyFollowing = await userFollowRepository.ExistsAsync(
            request.FollowerId, request.FolloweeId, cancellationToken);

        if (alreadyFollowing)
            throw new InvalidOperationException("Already following this user.");

        var follower = await userProfileRepository.GetByIdAsync(request.FollowerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Follower profile '{request.FollowerId}' not found.");

        var followee = await userProfileRepository.GetByIdAsync(request.FolloweeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Followee profile '{request.FolloweeId}' not found.");

        var follow = new UserFollow(request.FollowerId, request.FolloweeId);
        await userFollowRepository.CreateAsync(follow, cancellationToken);

        follower.IncrementFollowing();
        followee.IncrementFollowers();

        await userProfileRepository.UpdateAsync(follower, cancellationToken);
        await userProfileRepository.UpdateAsync(followee, cancellationToken);

        // Check if this follow makes the relationship mutual
        var isMutual = await userFollowRepository.ExistsAsync(
            request.FolloweeId, request.FollowerId, cancellationToken);

        await publishEndpoint.Publish(
            new UserFollowedEvent(
                FollowedUserId: followee.Id.ToString(),
                FollowedUsername: followee.Username,
                FollowerUserId: follower.Id.ToString(),
                FollowerUsername: follower.Username,
                IsMutual: isMutual,
                FollowerAvatarUrl: follower.AvatarUrl),
            cancellationToken);
    }
}
