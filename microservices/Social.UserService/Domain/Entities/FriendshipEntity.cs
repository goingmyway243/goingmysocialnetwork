using Social.UserService.Domain.Common;
using Social.UserService.Domain.Enums;

namespace Social.UserService.Domain.Entities
{
  public class FriendshipEntity : BaseEntity
  {
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
    public FriendshipStatus Status { get; set; }

    public FriendshipEntity(Guid userId, Guid friendId)
    {
      UserId = userId;
      FriendId = friendId;
    }
  }
}
