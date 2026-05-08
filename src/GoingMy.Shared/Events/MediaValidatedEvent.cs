namespace GoingMy.Shared.Events;

/// <summary>
/// Published by UploadService when all media files have been validated.
/// </summary>
public record MediaValidatedEvent
{
    public Guid CorrelationId { get; init; }
    public IReadOnlyList<string> MediaFileIds { get; init; } = [];
    public IReadOnlyList<MediaFileInfo> MediaFiles { get; init; } = [];
}

public record MediaFileInfo(string FileId, string Url, string ContentType);
