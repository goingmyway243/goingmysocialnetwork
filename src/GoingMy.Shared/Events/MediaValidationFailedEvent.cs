namespace GoingMy.Shared.Events;

/// <summary>
/// Published by UploadService when media validation fails.
/// </summary>
public record MediaValidationFailedEvent
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = null!;
}
