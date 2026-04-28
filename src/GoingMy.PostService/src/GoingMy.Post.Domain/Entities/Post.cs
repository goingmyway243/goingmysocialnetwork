namespace GoingMy.Post.Domain.Entities;

/// <summary>
/// Represents a social media post aggregate root.
/// </summary>
public class Post
{
    public string Id { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public User? Author { get; set; }

    /// <summary>
    /// Creates a new post instance.
    /// </summary>
    public Post(string id, string content, string userId, string username, DateTime createdAt)
    {
        Id = id;
        Content = content;
        UserId = userId;
        Username = username;
        CreatedAt = createdAt;
        Likes = 0;
        Comments = 0;
    }

    /// <summary>
    /// Updates the post content.
    /// </summary>
    public void Update(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
