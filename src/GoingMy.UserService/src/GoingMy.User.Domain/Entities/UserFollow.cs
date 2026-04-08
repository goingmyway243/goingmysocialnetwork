namespace GoingMy.User.Domain.Entities;

/// <summary>
/// Represents a directed follow relationship between two users.
/// Composite primary key: (FollowerId, FolloweeId).
/// </summary>
public class UserFollow
{
    public Guid FollowerId { get; set; }

    public Guid FolloweeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserFollow(Guid followerId, Guid followeeId)
    {
        FollowerId = followerId;
        FolloweeId = followeeId;
        CreatedAt = DateTime.UtcNow;
    }

    private UserFollow() { }
}
