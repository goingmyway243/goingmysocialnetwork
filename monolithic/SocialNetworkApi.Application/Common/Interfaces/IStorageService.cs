namespace SocialNetworkApi.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileName);
    Task<Stream> GetFileAsync(string fileName);
}
