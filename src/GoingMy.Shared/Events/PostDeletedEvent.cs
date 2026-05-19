namespace GoingMy.Shared.Events;

/// <summary>
/// Published by PostService when a post is deleted.
/// Consumed by SearchService to remove the post document from Elasticsearch.
/// </summary>
public record PostDeletedEvent
{
    public string PostId { get; init; } = null!;
    public string UserId { get; init; } = null!;
    public DateTime DeletedAt { get; init; }
}
