using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Domain.Entities;

public class FriendshipEntity : AuditedEntity
{
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid FriendId { get; set; }
    
    public FriendshipStatus Status { get; set; }
}
