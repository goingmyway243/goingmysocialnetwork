using Microsoft.Extensions.Hosting;
using SocialNetworkApi.Application.Common.Interfaces;

namespace SocialNetworkApi.Infrastructure.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _storagePath;

    public LocalStorageService(IHostEnvironment hostEnvironment)
    {
        _storagePath = Path.Combine(hostEnvironment.ContentRootPath, "Public");
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public Task DeleteFileAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> GetFileAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (File.Exists(filePath))
        {
            var result = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((Stream)result);
        }
        else
        {
            throw new FileNotFoundException("File not found.", fileName);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }
        return "http://localhost:5046/files/" + fileName;
    }
}
