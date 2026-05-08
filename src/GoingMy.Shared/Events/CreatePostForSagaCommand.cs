namespace GoingMy.Shared.Events;

/// <summary>
/// Command sent by the saga to PostService to create a post as part of the media upload flow.
/// </summary>
public record CreatePostForSagaCommand
{
    public Guid CorrelationId { get; init; }
    public string UserId { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Content { get; init; } = null!;
}
