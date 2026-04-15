using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GoingMy.Post.Application.Consumers;

/// <summary>
/// Handles <see cref="UserUpdatedEvent"/> from UserService.
/// Propagates profile changes (username, avatar, verification status) to all denormalized
/// Author fields on the user's posts via a single MongoDB bulk update.
/// </summary>
public class UserUpdatedEventConsumer(
    IPostRepository postRepository,
    ILogger<UserUpdatedEventConsumer> logger)
    : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var evt = context.Message;

        var updatedCount = await postRepository.BulkUpdateAuthorAsync(
            userId: evt.UserId.ToString(),
            username: evt.Username,
            firstName: evt.FirstName,
            lastName: evt.LastName,
            avatarUrl: evt.AvatarUrl,
            isVerified: evt.IsVerified,
            cancellationToken: context.CancellationToken);

        logger.LogInformation(
            "UserUpdatedEvent: synced Author fields on {Count} posts for user {UserId} ({Username}).",
            updatedCount, evt.UserId, evt.Username);
    }
}
