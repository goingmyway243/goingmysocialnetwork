using GoingMy.Upload.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GoingMy.Upload.Application.Consumers;

public class FileOrphanedEventConsumer(
    IMediaFileRepository repository,
    ILogger<FileOrphanedEventConsumer> logger)
    : IConsumer<FileOrphanedEvent>
{
    public async Task Consume(ConsumeContext<FileOrphanedEvent> context)
    {
        var fileId = context.Message.FileId;

        var mediaFile = await repository.GetByIdAsync(fileId, context.CancellationToken);
        if (mediaFile is null)
        {
            logger.LogDebug("FileOrphanedEvent: file {FileId} not found, skipping.", fileId);
            return;
        }

        mediaFile.MarkAsOrphaned();
        await repository.UpdateAsync(mediaFile, context.CancellationToken);

        logger.LogInformation("Marked media file {FileId} as orphaned for cleanup.", fileId);
    }
}
