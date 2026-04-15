namespace GoingMy.Shared.Events;

/// <summary>
/// Published by UserService when a user profile is deleted.
/// Consumed by PostService (tombstone posts) and ChatService (remove from active conversations).
/// </summary>
public record UserDeletedEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public DateTime DeletedAt { get; init; }
}
