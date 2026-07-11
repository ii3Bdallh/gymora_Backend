using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.Interface.Service.Shared
{
    using Microsoft.AspNetCore.Http;

    namespace Application.Interface.Service.Shared
    {
        public interface IStorageService
        {
            Task<string> UploadFileToStorageAsync(IFormFile file, CancellationToken cancellationToken = default);

            string GenerateUrlToAccessFileAsync(string fileName, CancellationToken cancellationToken = default);

            /// <summary>
            /// Delete a specific file from Storage
            /// </summary>
            Task<bool> DeleteFileFromStorageAsync(string fileName, CancellationToken cancellationToken = default);

            /// <summary>
            /// Delete all files in a collection/folder from Storage
            /// </summary>
            Task<bool> DeleteCollectionFromStorageAsync(string collectionPath, CancellationToken cancellationToken = default);
        }
    }
}


