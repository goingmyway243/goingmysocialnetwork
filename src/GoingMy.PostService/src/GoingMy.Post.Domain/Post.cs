namespace GoingMy.Post.Domain;

/// <summary>
/// Represents a social media post aggregate root.
/// </summary>
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Creates a new post instance.
    /// </summary>
    public Post(int id, string title, string content, string userId, string username, DateTime createdAt)
    {
        Id = id;
        Title = title;
        Content = content;
        UserId = userId;
        Username = username;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Private constructor for entity framework.
    /// </summary>
    private Post()
    {
    }

    /// <summary>
    /// Updates the post content.
    /// </summary>
    public void Update(string title, string content)
    {
        Title = title;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
