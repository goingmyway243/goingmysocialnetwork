namespace GoingMy.Shared.Events;

/// <summary>
/// Published by UserService when a user profile is updated.
/// Consumed by PostService (sync Author fields) and ChatService (sync participant names).
/// </summary>
public record UserUpdatedEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? AvatarUrl { get; init; }
    public bool IsVerified { get; init; }
    public DateTime UpdatedAt { get; init; }
}
