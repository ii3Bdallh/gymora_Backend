using Microsoft.AspNetCore.Http;

namespace Application.Interface.Service.Shared
{
    public interface IStorageService
    {
        /// <summary>
        /// رفع ملف مع تحديد نوعه (Public أو Private)
        /// </summary>
        Task<string> UploadFileToStorageAsync(
            IFormFile file, 
            bool isPublic, 
            string entityType,           // مثال: "Gym", "Exercise", "Receipt"
            CancellationToken cancellationToken = default);

        /// <summary>
        /// إرجاع رابط صالح للملف (Public → مباشر، Private → Signed URL)
        /// </summary>
        string GetFileAccessUrl(string storedFileName, bool isPublic);

        Task<bool> DeleteFileFromStorageAsync(string storedFileName, CancellationToken cancellationToken = default);

        Task<bool> DeleteCollectionFromStorageAsync(string collectionPath, CancellationToken cancellationToken = default);
    }
}