namespace GoingMy.Post.Domain.Entities;

/// <summary>
/// Represents a denormalized snapshot of a user's profile embedded in a post.
/// Synced from UserService via Kafka events.
/// </summary>
public record User
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsVerified { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
