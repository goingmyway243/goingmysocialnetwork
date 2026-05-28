using GoingMy.Upload.Domain.Storage;
using Microsoft.Extensions.Options;

namespace GoingMy.Upload.Infrastructure.Storage;

public class LocalFileStorageProvider(IOptions<StorageSettings> options) : IFileStorageProvider
{
    private readonly StorageSettings _settings = options.Value;

    public async Task<(string fileKey, string url)> UploadAsync(
        Stream stream,
        string fileName,
        string baseFolder,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName);
        var fileKey = $"{Guid.NewGuid():N}{extension}";
        var basePath = Path.Combine(_settings.BasePath, baseFolder);
        var fullPath = Path.Combine(basePath, fileKey);

        Directory.CreateDirectory(basePath);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, ct);

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/{baseFolder}/{fileKey}";
        return (fileKey, url);
    }

    public Task DeleteAsync(string fileKey, string baseFolder, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_settings.BasePath, baseFolder, fileKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
