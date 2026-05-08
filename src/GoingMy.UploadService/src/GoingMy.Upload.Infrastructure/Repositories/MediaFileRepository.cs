using GoingMy.Upload.Domain.Entities;
using GoingMy.Upload.Domain.Enums;
using GoingMy.Upload.Domain.Repositories;
using GoingMy.Upload.Infrastructure.Data;
using MongoDB.Driver;

namespace GoingMy.Upload.Infrastructure.Repositories;

public class MediaFileRepository(UploadDbContext context) : IMediaFileRepository
{
    public async Task<MediaFile?> GetByIdAsync(string id, CancellationToken ct = default)
        => await context.MediaFiles.Find(f => f.Id == id).FirstOrDefaultAsync(ct);

    public async Task<MediaFile?> GetByFileKeyAsync(string fileKey, CancellationToken ct = default)
        => await context.MediaFiles.Find(f => f.FileKey == fileKey).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<MediaFile>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await context.MediaFiles.Find(f => idList.Contains(f.Id)).ToListAsync(ct);
    }

    public async Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken ct = default)
    {
        await context.MediaFiles.InsertOneAsync(mediaFile, cancellationToken: ct);
        return mediaFile;
    }

    public async Task UpdateAsync(MediaFile mediaFile, CancellationToken ct = default)
    {
        await context.MediaFiles.ReplaceOneAsync(f => f.Id == mediaFile.Id, mediaFile, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<MediaFile>> GetOrphanedAsync(DateTime olderThan, CancellationToken ct = default)
        => await context.MediaFiles
            .Find(f => (f.Status == UploadStatus.Orphaned) && f.CreatedAt < olderThan)
            .ToListAsync(ct);
}
