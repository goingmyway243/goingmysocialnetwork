using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class ChatroomEntity : BaseEntity
{
    public string ChatroomName { get; set; } = string.Empty;
    
    [BsonRepresentation(BsonType.String)]
    public List<Guid> ParticipantIds = new List<Guid>();
}
