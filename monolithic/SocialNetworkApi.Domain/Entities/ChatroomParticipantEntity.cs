﻿using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class ChatroomParticipantEntity : BaseEntity
{
    public Guid ChatroomId { get; set; }
    public Guid UserId { get; set; }

    // Relationship
    public ChatroomEntity Chatroom { get; set; } = null!;
}
