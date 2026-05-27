namespace GoingMy.Shared.Events;

/// <summary>
/// Published by PostService when an existing post is updated.
/// Consumed by SearchService to update the post document in Elasticsearch.
/// </summary>
public record PostUpdatedEvent
{
    public string PostId { get; init; } = null!;
    public string UserId { get; init; } = null!;
    public string Content { get; init; } = null!;
    public IReadOnlyList<MediaAttachmentInfo> MediaAttachments { get; init; } = [];
    public DateTime UpdatedAt { get; init; }
}
