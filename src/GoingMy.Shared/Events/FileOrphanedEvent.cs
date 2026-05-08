namespace GoingMy.Shared.Events;

/// <summary>
/// Published when a file in UploadService is no longer needed (e.g., user replaced their avatar).
/// UploadService consumes this event to mark the file as orphaned for cleanup.
/// </summary>
public record FileOrphanedEvent
{
    public string FileId { get; init; } = null!;
}
