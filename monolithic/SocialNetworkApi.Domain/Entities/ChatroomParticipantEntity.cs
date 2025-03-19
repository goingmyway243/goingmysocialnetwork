using System.ComponentModel.DataAnnotations.Schema;
using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

[Table("ChatroomParticipants")]
public class ChatroomParticipantEntity : BaseEntity
{
    public Guid ChatroomId { get; set; }
    public Guid UserId { get; set; }

    // Relationship
    public ChatroomEntity Chatroom { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
}
