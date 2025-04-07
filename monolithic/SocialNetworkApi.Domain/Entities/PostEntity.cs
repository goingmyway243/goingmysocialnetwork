using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class PostEntity : AuditedEntity
{
    public string Caption { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid? SharePostId { get; set; }
    
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public List<ContentEntity> Contents { get; set; } = new List<ContentEntity>();
}
