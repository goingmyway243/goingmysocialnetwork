using GoingMy.Chat.Application.Commands;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GoingMy.Chat.Application.Consumers;

/// <summary>
/// Handles <see cref="UserFollowedEvent"/> and auto-creates a conversation
/// when the follow relationship becomes mutual (tracked via <see cref="UserFollowedEvent.IsMutual"/>).
/// </summary>
public class MutualFollowConversationConsumer(
    ISender sender,
    ILogger<MutualFollowConversationConsumer> logger)
    : IConsumer<UserFollowedEvent>
{
    public async Task Consume(ConsumeContext<UserFollowedEvent> context)
    {
        var evt = context.Message;

        if (!Guid.TryParse(evt.FollowerUserId, out _)
            || !Guid.TryParse(evt.FollowedUserId, out _))
        {
            logger.LogWarning(
                "UserFollowedEvent payload has invalid IDs. FollowerUserId={FollowerUserId}, FollowedUserId={FollowedUserId}",
                evt.FollowerUserId,
                evt.FollowedUserId);
            return;
        }

        // Only create conversation if the follow relationship is mutual
        if (!evt.IsMutual)
        {
            logger.LogDebug(
                "Follow is not yet mutual between {FollowerId} and {FollowedId}.",
                evt.FollowerUserId,
                evt.FollowedUserId);
            return;
        }

        var conversation = await sender.Send(new CreateConversationCommand(
            InitiatorId: evt.FollowerUserId,
            InitiatorUsername: evt.FollowerUsername,
            RecipientId: evt.FollowedUserId,
            RecipientUsername: evt.FollowedUsername),
            context.CancellationToken);

        logger.LogInformation(
            "Auto-created or reused conversation {ConversationId} for mutual follow between {FollowerId} and {FollowedId}.",
            conversation.Id,
            evt.FollowerUserId,
            evt.FollowedUserId);
    }
}
