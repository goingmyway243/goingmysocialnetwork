namespace GoingMy.Shared.Events;

/// <summary>
/// Command sent by the saga state machine to UploadService to validate that the specified media files
/// exist, are Ready, and belong to the requesting user.
/// </summary>
public record ValidateMediaForSagaCommand
{
    public Guid CorrelationId { get; init; }
    public string UserId { get; init; } = null!;
    public IReadOnlyList<string> MediaFileIds { get; init; } = [];
}
