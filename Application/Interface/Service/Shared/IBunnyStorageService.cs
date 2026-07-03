using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.Interface.Service.Shared
{
    public interface IBunnyStorageService
    {
        Task<string> UploadFileToBunnyStorageAsync(IFormFile file, CancellationToken cancellationToken = default);

        string GenerateUrlToAccessFileAsync(string fileName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete a specific file from Bunny Storage
        /// </summary>
        Task<bool> DeleteFileFromBunnyStorageAsync(string fileName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Delete all files in a collection/folder from Bunny Storage
        /// </summary>
        Task<bool> DeleteCollectionFromBunnyStorageAsync(string collectionPath, CancellationToken cancellationToken = default);
    }
}


