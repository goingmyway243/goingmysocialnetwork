using GoingMy.Shared.Events;
using GoingMy.User.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GoingMy.User.Application.Consumers;

/// <summary>
/// Consumes <see cref="UserRegisteredEvent"/> published by AuthService when a new account is created.
/// Upserts the social profile — idempotent so duplicate deliveries are safe.
/// </summary>
public class UserRegisteredEventConsumer(
    IMediator mediator,
    ILogger<UserRegisteredEventConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "UserRegisteredEvent received for user {UserId} ({Username}). Upserting social profile.",
            evt.UserId, evt.Username);

        await mediator.Send(
            new CreateUserProfileCommand(evt.UserId, evt.Username, evt.FirstName, evt.LastName),
            context.CancellationToken);
    }
}
