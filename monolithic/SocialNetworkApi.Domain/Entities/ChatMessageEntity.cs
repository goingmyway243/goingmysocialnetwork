using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class ChatMessageEntity : AuditedEntity
{
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid ChatroomId { get; set; }

    // Relationship
    public UserEntity User { get; set; } = null!;
    public ChatroomEntity Chatroom { get; set; } = null!;
}
