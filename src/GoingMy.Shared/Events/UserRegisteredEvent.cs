namespace GoingMy.Shared.Events;

/// <summary>
/// Published by AuthService to RabbitMQ immediately after a new account is created.
/// Consumed by UserService to bootstrap the social profile.
/// </summary>
public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public DateTime RegisteredAt { get; init; }
}
