using Microsoft.AspNetCore.Http;

namespace Application.Interface.Service.Shared;

public interface IStorageService
{
    Task<string> UploadFileToStorageAsync(IFormFile file, CancellationToken cancellationToken = default);

    string GenerateUrlToAccessFileAsync(string fileName, CancellationToken cancellationToken = default);

    Task<bool> DeleteFileFromStorageAsync(string fileName, CancellationToken cancellationToken = default);

    Task<bool> DeleteCollectionFromStorageAsync(string collectionPath, CancellationToken cancellationToken = default);
}
