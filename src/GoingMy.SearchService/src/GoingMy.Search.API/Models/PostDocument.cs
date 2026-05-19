namespace GoingMy.Search.API.Models;

public class PostDoc
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyList<string>? MediaAttachments { get; set; }
    public SuggestField? Suggest { get; set; }
}
