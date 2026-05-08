using GoingMy.Upload.Domain.Repositories;
using GoingMy.Upload.Domain.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoingMy.Upload.Infrastructure.Workers;

public class OrphanedMediaCleanupWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OrphanedMediaCleanupWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private static readonly TimeSpan OrphanThreshold = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupOrphanedFilesAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CleanupOrphanedFilesAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMediaFileRepository>();
            var storage = scope.ServiceProvider.GetRequiredService<IFileStorageProvider>();

            var cutoff = DateTime.UtcNow - OrphanThreshold;
            var orphaned = await repository.GetOrphanedAsync(cutoff, ct);

            foreach (var file in orphaned)
            {
                try
                {
                    await storage.DeleteAsync(file.FileKey, ct);
                    file.MarkAsDeleted();
                    await repository.UpdateAsync(file, ct);
                    logger.LogInformation("Deleted orphaned media file {FileId}", file.Id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to delete orphaned media file {FileId}", file.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during orphaned media cleanup");
        }
    }
}
