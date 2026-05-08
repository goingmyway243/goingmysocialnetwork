using GoingMy.Upload.Application.Dtos;
using GoingMy.Upload.Domain.Repositories;
using MediatR;

namespace GoingMy.Upload.Application.Queries;

public record GetMediaFileQuery(string FileId) : IRequest<MediaFileDto?>;

public class GetMediaFileQueryHandler(IMediaFileRepository repository)
    : IRequestHandler<GetMediaFileQuery, MediaFileDto?>
{
    public async Task<MediaFileDto?> Handle(GetMediaFileQuery request, CancellationToken ct)
    {
        var mediaFile = await repository.GetByIdAsync(request.FileId, ct);
        if (mediaFile is null) return null;

        return new MediaFileDto(
            Id: mediaFile.Id,
            Url: mediaFile.Url,
            OriginalFileName: mediaFile.OriginalFileName,
            ContentType: mediaFile.ContentType,
            FileSizeBytes: mediaFile.FileSizeBytes,
            Width: mediaFile.Width,
            Height: mediaFile.Height,
            DurationSeconds: mediaFile.DurationSeconds,
            Purpose: mediaFile.Purpose,
            Status: mediaFile.Status,
            CreatedAt: mediaFile.CreatedAt);
    }
}
