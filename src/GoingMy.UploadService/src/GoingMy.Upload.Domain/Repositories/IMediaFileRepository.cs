using GoingMy.Upload.Domain.Entities;

namespace GoingMy.Upload.Domain.Repositories;

public interface IMediaFileRepository
{
    Task<MediaFile?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<MediaFile?> GetByFileKeyAsync(string fileKey, CancellationToken ct = default);
    Task<IReadOnlyList<MediaFile>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default);
    Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken ct = default);
    Task UpdateAsync(MediaFile mediaFile, CancellationToken ct = default);
    Task<IReadOnlyList<MediaFile>> GetOrphanedAsync(DateTime olderThan, CancellationToken ct = default);
}
