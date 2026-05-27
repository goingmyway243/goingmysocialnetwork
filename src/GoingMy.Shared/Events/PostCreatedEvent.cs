namespace GoingMy.Shared.Events;

/// <summary>
/// Published by PostService when a new post is created.
/// Consumed by SearchService to index the post in Elasticsearch.
/// </summary>
public record PostCreatedEvent
{
    public string PostId { get; init; } = null!;
    public string UserId { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Content { get; init; } = null!;
    public IReadOnlyList<MediaAttachmentInfo> MediaAttachments { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}
