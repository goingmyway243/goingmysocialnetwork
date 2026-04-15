namespace GoingMy.Shared.Events;

/// <summary>
/// Published by UserService when a new user profile is created.
/// Consumed by PostService and ChatService to pre-cache user data.
/// </summary>
public record UserCreatedEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? AvatarUrl { get; init; }
    public bool IsVerified { get; init; }
    public DateTime CreatedAt { get; init; }
}
