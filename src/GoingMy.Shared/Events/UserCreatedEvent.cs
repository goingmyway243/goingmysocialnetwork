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
    public int Gender { get; init; }
    public bool IsVerified { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
