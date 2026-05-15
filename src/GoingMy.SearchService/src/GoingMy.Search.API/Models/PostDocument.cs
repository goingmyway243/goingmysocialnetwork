namespace GoingMy.Search.API.Models;

public record PostDoc{
  public string Id { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public int Likes { get; set; }
  public int Comments { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public bool UserHasLiked { get; set; }
  public IReadOnlyList<string>? MediaAttachments { get; set; }
}
