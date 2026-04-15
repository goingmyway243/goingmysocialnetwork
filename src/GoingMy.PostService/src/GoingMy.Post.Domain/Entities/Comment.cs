namespace GoingMy.Post.Domain.Entities;

/// <summary>
/// Represents a user comment on a post.
/// </summary>
public class Comment
{
    public string Id { get; set; } = null!;
    public string PostId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Comment(string id, string postId, string userId, string username, string content, DateTime createdAt)
    {
        Id = id;
        PostId = postId;
        UserId = userId;
        Username = username;
        Content = content;
        CreatedAt = createdAt;
    }

    private Comment() { }

    public void Update(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
