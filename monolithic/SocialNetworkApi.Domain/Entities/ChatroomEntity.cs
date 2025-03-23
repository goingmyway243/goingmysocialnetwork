using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class ChatroomEntity : BaseEntity
{
    public string ChatroomName { get; set; } = string.Empty;

    // Relationship
    public virtual List<ChatroomParticipantEntity> Participants { get; set; } = new List<ChatroomParticipantEntity>();
    public virtual List<ChatMessageEntity> ChatMessages { get; set; } = new List<ChatMessageEntity>();
}
