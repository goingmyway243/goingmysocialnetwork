namespace GoingMy.Shared.Events;

/// <summary>
/// Published when the post-with-media saga completes, either successfully or with failure.
/// </summary>
public record PostWithMediaSagaCompletedEvent
{
    public Guid CorrelationId { get; init; }
    public string UserId { get; init; } = null!;
    public string? PostId { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}