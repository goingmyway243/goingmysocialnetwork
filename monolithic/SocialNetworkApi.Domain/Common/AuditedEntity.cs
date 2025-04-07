using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialNetworkApi.Domain.Common;

public class AuditedEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid CreatedBy { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid? ModifiedBy { get; set; }

    public AuditedEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
