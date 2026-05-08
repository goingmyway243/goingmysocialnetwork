using GoingMy.Upload.Application.Commands;
using GoingMy.Upload.Application.Dtos;
using GoingMy.Upload.Domain.Repositories;
using MediatR;

namespace GoingMy.Upload.Application.Queries;

public record GetMediaFileByKeyQuery(string FileKey) : IRequest<MediaFileDto?>;

public class GetMediaFileByKeyQueryHandler(IMediaFileRepository repository)
    : IRequestHandler<GetMediaFileByKeyQuery, MediaFileDto?>
{
    public async Task<MediaFileDto?> Handle(GetMediaFileByKeyQuery request, CancellationToken ct)
    {
        var mediaFile = await repository.GetByFileKeyAsync(request.FileKey, ct);
        if (mediaFile is null) return null;

        return UploadFileCommandHandler.MapToDto(mediaFile);
    }
}
