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
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CoverUrl { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public int Gender { get; init; }
    public string? Location { get; init; }
    public string? WebsiteUrl { get; init; }
    public int FollowersCount { get; init; }
    public int FollowingCount { get; init; }
    public int PostsCount { get; init; }
    public bool IsVerified { get; init; }
    public bool IsPrivate { get; init; }
    public bool IsActive { get; init; }
    public List<string> Interests { get; init; } = new List<string>();
    public DateTime? UpdatedAt { get; init; }
}
