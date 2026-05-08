namespace GoingMy.Shared.Events;

/// <summary>
/// Published by PostService when post creation fails during the saga.
/// </summary>
public record PostCreationFailedEvent
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = null!;
}
