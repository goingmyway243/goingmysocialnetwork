using SocialNetworkApi.Domain.Common;

namespace SocialNetworkApi.Domain.Entities;

public class LikeEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }

    // Relationships
    public UserEntity User { get; set; } = null!;
    public PostEntity Post { get; set; } = null!;
}
