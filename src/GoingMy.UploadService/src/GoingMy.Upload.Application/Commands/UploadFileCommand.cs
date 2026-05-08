using GoingMy.Upload.Application.Dtos;
using GoingMy.Upload.Domain.Entities;
using GoingMy.Upload.Domain.Enums;
using GoingMy.Upload.Domain.Repositories;
using GoingMy.Upload.Domain.Storage;
using MediatR;

namespace GoingMy.Upload.Application.Commands;

public record UploadFileCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string UserId,
    MediaPurpose Purpose
) : IRequest<MediaFileDto>;

public class UploadFileCommandHandler(
    IMediaFileRepository repository,
    IFileStorageProvider storage)
    : IRequestHandler<UploadFileCommand, MediaFileDto>
{
    private static readonly HashSet<string> AllowedImageTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private static readonly HashSet<string> AllowedVideoTypes = ["video/mp4", "video/webm"];
    private const long MaxImageBytes = 10 * 1024 * 1024;   // 10 MB
    private const long MaxVideoBytes = 100 * 1024 * 1024;  // 100 MB

    public async Task<MediaFileDto> Handle(UploadFileCommand request, CancellationToken ct)
    {
        ValidateFile(request.ContentType, request.FileSizeBytes);

        var (fileKey, url) = await storage.UploadAsync(request.FileStream, request.FileName, request.ContentType, ct);

        var mediaFile = new MediaFile(
            id: Guid.NewGuid().ToString("N"),
            fileKey: fileKey,
            originalFileName: request.FileName,
            contentType: request.ContentType,
            fileSizeBytes: request.FileSizeBytes,
            uploadedByUserId: request.UserId,
            purpose: request.Purpose,
            url: url);

        var saved = await repository.AddAsync(mediaFile, ct);
        return MapToDto(saved);
    }

    private static void ValidateFile(string contentType, long sizeBytes)
    {
        if (AllowedImageTypes.Contains(contentType))
        {
            if (sizeBytes > MaxImageBytes)
                throw new InvalidOperationException($"Image exceeds maximum size of {MaxImageBytes / 1024 / 1024} MB.");
        }
        else if (AllowedVideoTypes.Contains(contentType))
        {
            if (sizeBytes > MaxVideoBytes)
                throw new InvalidOperationException($"Video exceeds maximum size of {MaxVideoBytes / 1024 / 1024} MB.");
        }
        else
        {
            throw new InvalidOperationException($"Unsupported file type: {contentType}. Allowed: jpg, png, webp, gif, mp4, webm.");
        }
    }

    internal static MediaFileDto MapToDto(MediaFile f) => new(
        Id: f.Id,
        Url: f.Url,
        OriginalFileName: f.OriginalFileName,
        ContentType: f.ContentType,
        FileSizeBytes: f.FileSizeBytes,
        Width: f.Width,
        Height: f.Height,
        DurationSeconds: f.DurationSeconds,
        Purpose: f.Purpose,
        Status: f.Status,
        CreatedAt: f.CreatedAt);
}
