using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class PostEntity : AuditedEntity
{
    public string Caption { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? SharePostId { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }

    // Relationships
    public UserEntity User { get; set; } = null!;
    public PostEntity SharePost { get; set; } = null!;
    public List<ContentEntity> Contents { get; set; } = new List<ContentEntity>();
    public List<CommentEntity> CommentEntities { get; set; } = new List<CommentEntity>();
    public List<LikeEntity> Like { get; set; } = new List<LikeEntity>();
}
