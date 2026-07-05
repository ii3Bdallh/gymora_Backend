namespace Application.Interface.Service.Shared;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);

    Task DeleteAsync(string fileUrl);

    Task<Stream?> DownloadAsync(string fileUrl);
}
