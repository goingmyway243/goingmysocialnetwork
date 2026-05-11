using GoingMy.Notification.Domain.Enums;

namespace GoingMy.Notification.Domain.Entities;

public class Notification
{
    public string Id { get; set; } = null!;
    public string RecipientUserId { get; set; } = null!;
    public string ActorUserId { get; set; } = null!;
    public string ActorUsername { get; set; } = null!;
    public string? ActorAvatarUrl { get; set; }
    public NotificationType Type { get; set; }
    public string? ReferenceId { get; set; }
    public string? ReferencePreview { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public Notification(
        string id,
        string recipientUserId,
        string actorUserId,
        string actorUsername,
        NotificationType type,
        DateTime createdAt,
        string? actorAvatarUrl = null,
        string? referenceId = null,
        string? referencePreview = null)
    {
        Id = id;
        RecipientUserId = recipientUserId;
        ActorUserId = actorUserId;
        ActorUsername = actorUsername;
        ActorAvatarUrl = actorAvatarUrl;
        Type = type;
        ReferenceId = referenceId;
        ReferencePreview = referencePreview;
        IsRead = false;
        CreatedAt = createdAt;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
