using GoingMy.Chat.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GoingMy.Chat.Application.Consumers;

/// <summary>
/// Handles <see cref="UserUpdatedEvent"/> from UserService.
/// Propagates username changes to the denormalized participant username list
/// in all conversations the user is a member of.
/// </summary>
public class UserUpdatedEventConsumer(
    IConversationRepository conversationRepository,
    ILogger<UserUpdatedEventConsumer> logger)
    : IConsumer<UserUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var evt = context.Message;

        var updatedCount = await conversationRepository.BulkUpdateParticipantUsernameAsync(
            userId: evt.UserId.ToString(),
            newUsername: evt.Username,
            cancellationToken: context.CancellationToken);

        logger.LogInformation(
            "UserUpdatedEvent: synced participant username in {Count} conversations for user {UserId} ({Username}).",
            updatedCount, evt.UserId, evt.Username);
    }
}
