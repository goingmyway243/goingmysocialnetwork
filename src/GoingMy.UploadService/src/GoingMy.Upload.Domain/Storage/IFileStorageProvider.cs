namespace GoingMy.Upload.Domain.Storage;

public interface IFileStorageProvider
{
    Task<(string fileKey, string url)> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string fileKey, CancellationToken ct = default);
}
