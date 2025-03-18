using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class ChatroomEntity : BaseEntity
{
    public string ChatroomName { get; set; } = string.Empty;

    // Relationships
    public virtual List<UserEntity> ChatMembers { get; set; } = new List<UserEntity>();
    public virtual List<ChatMessageEntity> Messages { get; set; } = new List<ChatMessageEntity>();
}
