using GoingMy.Upload.Domain.Enums;

namespace GoingMy.Upload.Application.Dtos;

public record MediaFileDto(
    string Id,
    string Url,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    int? Width,
    int? Height,
    double? DurationSeconds,
    MediaPurpose Purpose,
    UploadStatus Status,
    DateTime CreatedAt
);
