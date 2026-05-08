namespace GoingMy.Shared.Events;

/// <summary>
/// Published by the frontend (via PostService) to initiate the post-with-media saga.
/// </summary>
public record PostWithMediaRequestedEvent
{
    public Guid CorrelationId { get; init; }
    public string UserId { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Content { get; init; } = null!;
    public IReadOnlyList<string> MediaFileIds { get; init; } = [];
}
