using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Domain.Entities;

public class FriendshipEntity : AuditedEntity
{
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
    public FriendshipStatus Status { get; set; }
}
