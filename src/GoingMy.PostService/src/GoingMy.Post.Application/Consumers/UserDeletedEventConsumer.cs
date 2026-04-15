using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GoingMy.Post.Application.Consumers;

/// <summary>
/// Handles <see cref="UserDeletedEvent"/> from UserService.
/// Tombstones all posts by the deleted user — Author fields are replaced with
/// "[deleted]" placeholders so post history is retained without exposing the old profile.
/// </summary>
public class UserDeletedEventConsumer(
    IPostRepository postRepository,
    ILogger<UserDeletedEventConsumer> logger)
    : IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var evt = context.Message;

        var updatedCount = await postRepository.MarkPostsAsDeletedUserAsync(
            userId: evt.UserId.ToString(),
            cancellationToken: context.CancellationToken);

        logger.LogInformation(
            "UserDeletedEvent: tombstoned {Count} posts for deleted user {UserId} ({Username}).",
            updatedCount, evt.UserId, evt.Username);
    }
}
