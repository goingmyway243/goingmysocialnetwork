using GoingMy.Upload.Domain.Enums;

namespace GoingMy.Upload.Domain.Entities;

public class MediaFile
{
    public string Id { get; set; } = null!;
    public string FileKey { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? DurationSeconds { get; set; }
    public string UploadedByUserId { get; set; } = null!;
    public MediaPurpose Purpose { get; set; }
    public UploadStatus Status { get; set; }
    public string Url { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public MediaFile(
        string id,
        string fileKey,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string uploadedByUserId,
        MediaPurpose purpose,
        string url)
    {
        Id = id;
        FileKey = fileKey;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        UploadedByUserId = uploadedByUserId;
        Purpose = purpose;
        Url = url;
        Status = UploadStatus.Ready;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsOrphaned()
    {
        Status = UploadStatus.Orphaned;
    }

    public void MarkAsDeleted()
    {
        Status = UploadStatus.Deleted;
        DeletedAt = DateTime.UtcNow;
    }
}
