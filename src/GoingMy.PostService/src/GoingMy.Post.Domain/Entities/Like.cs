namespace GoingMy.Post.Domain.Entities;

/// <summary>
/// Represents a user liking a post. Enforces one like per user per post.
/// </summary>
public class Like
{
    public string Id { get; set; } = null!;
    public string PostId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Like(string id, string postId, string userId, string username, DateTime createdAt)
    {
        Id = id;
        PostId = postId;
        UserId = userId;
        Username = username;
        CreatedAt = createdAt;
    }

    private Like() { }
}
