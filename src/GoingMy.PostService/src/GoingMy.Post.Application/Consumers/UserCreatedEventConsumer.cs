using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GoingMy.Post.Application.Consumers;

/// <summary>
/// Handles <see cref="UserCreatedEvent"/> from UserService.
/// No-op for PostService — posts are only created when the user actually creates one,
/// so no pre-population is needed. The consumer exists to acknowledge the message
/// and allow future extensions (e.g. creating a user profile cache entry).
/// </summary>
public class UserCreatedEventConsumer(ILogger<UserCreatedEventConsumer> logger)
    : IConsumer<UserCreatedEvent>
{
    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        logger.LogInformation(
            "UserCreatedEvent received for user {UserId} ({Username}). No post sync required.",
            context.Message.UserId, context.Message.Username);

        return Task.CompletedTask;
    }
}
