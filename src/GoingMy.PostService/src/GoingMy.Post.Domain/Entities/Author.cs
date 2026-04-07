namespace GoingMy.Post.Domain.Entities;

/// <summary>
/// Represents a denormalized user profile for posts (synced from AuthService).
/// </summary>
public record Author
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsVerified { get; set; }

    /// <summary>
    /// Gets the full name of the author.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
