using System;

namespace SocialNetworkApi.Application.Common.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public string Caption { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? SharePostId { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}
