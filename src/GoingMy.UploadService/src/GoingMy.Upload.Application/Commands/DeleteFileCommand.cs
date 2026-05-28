using GoingMy.Upload.Domain.Repositories;
using GoingMy.Upload.Domain.Storage;
using MediatR;

namespace GoingMy.Upload.Application.Commands;

public record DeleteFileCommand(string FileId, string UserId) : IRequest;

public class DeleteFileCommandHandler(
    IMediaFileRepository repository,
    IFileStorageProvider storage)
    : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(DeleteFileCommand request, CancellationToken ct)
    {
        var mediaFile = await repository.GetByIdAsync(request.FileId, ct)
            ?? throw new InvalidOperationException($"Media file '{request.FileId}' not found.");

        if (mediaFile.UploadedByUserId != request.UserId)
            throw new UnauthorizedAccessException("You do not have permission to delete this file.");

        await storage.DeleteAsync(mediaFile.FileKey, mediaFile.Purpose.ToString(), ct);
        mediaFile.MarkAsDeleted();
        await repository.UpdateAsync(mediaFile, ct);
    }
}
