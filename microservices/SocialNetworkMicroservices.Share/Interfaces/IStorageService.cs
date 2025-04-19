namespace SocialNetworkMicroservices.Share.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "");
    Task DeleteFileAsync(string fileName);
    Task<Stream> GetFileAsync(string fileName);
}
