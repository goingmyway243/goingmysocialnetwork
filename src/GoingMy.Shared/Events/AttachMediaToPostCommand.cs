namespace GoingMy.Shared.Events;

/// <summary>
/// Command sent by the saga to PostService to attach media files to a post.
/// </summary>
public record AttachMediaToPostCommand
{
    public Guid CorrelationId { get; init; }
    public string PostId { get; init; } = null!;
    public IReadOnlyList<MediaFileInfo> MediaFiles { get; init; } = [];
}
