using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class ChatMessageEntity : AuditedEntity
{
    public string Message { get; set; } = string.Empty;
    
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid ChatroomId { get; set; }
}
