namespace GoingMy.Shared.Events;

/// <summary>
/// Published by PostService when media has been attached to the post.
/// </summary>
public record MediaAttachedToPostEvent
{
    public Guid CorrelationId { get; init; }
    public string PostId { get; init; } = null!;
}
