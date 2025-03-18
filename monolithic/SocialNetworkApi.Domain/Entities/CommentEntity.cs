using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class CommentEntity : AuditedEntity
{
    public string Comment { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }

    // Relationship
    public UserEntity User { get; set; } = null!;
    public PostEntity Post { get; set; } = null!;
}
