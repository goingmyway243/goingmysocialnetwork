using GoingMy.Chat.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GoingMy.Chat.Application.Consumers;

/// <summary>
/// Handles <see cref="UserDeletedEvent"/> from UserService.
/// Removes the deleted user from participant lists in all their conversations.
/// Message history is retained for the remaining participants.
/// </summary>
public class UserDeletedEventConsumer(
    IConversationRepository conversationRepository,
    ILogger<UserDeletedEventConsumer> logger)
    : IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var evt = context.Message;

        var updatedCount = await conversationRepository.RemoveParticipantAsync(
            userId: evt.UserId.ToString(),
            cancellationToken: context.CancellationToken);

        logger.LogInformation(
            "UserDeletedEvent: removed user {UserId} ({Username}) from {Count} conversations.",
            evt.UserId, evt.Username, updatedCount);
    }
}
