using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Application.Common.DTOs;

public class FriendshipDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }

    public UserDto? User { get; set; }
}
