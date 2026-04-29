namespace GoingMy.User.Domain.Entities;

/// <summary>
/// Represents a unidirectional block relationship between two users.
/// When BlockerId blocks BlockeeId, BlockeeId cannot send messages to BlockerId.
/// Composite primary key: (BlockerId, BlockeeId).
/// </summary>
public class UserBlock
{
    public Guid BlockerId { get; set; }

    public Guid BlockeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserBlock(Guid blockerId, Guid blockeeId)
    {
        BlockerId = blockerId;
        BlockeeId = blockeeId;
        CreatedAt = DateTime.UtcNow;
    }

    private UserBlock() { }
}
