using GoingMy.Search.API.Enums;

namespace GoingMy.Search.API.Models;

public class UserDoc
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.Other;
    public string? Location { get; set; }
    public string? WebsiteUrl { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int PostsCount { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsActive { get; set; }
    public List<string> Interests { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
