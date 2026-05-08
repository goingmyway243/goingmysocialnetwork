namespace GoingMy.Shared.Events;

/// <summary>
/// Published by PostService when the post record has been created (without media attached yet).
/// </summary>
public record PostCreatedForSagaEvent
{
    public Guid CorrelationId { get; init; }
    public string PostId { get; init; } = null!;
}
