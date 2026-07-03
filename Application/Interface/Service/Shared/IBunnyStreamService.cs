using Application.DTO.Bunny;
using Application.Service.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Service.Shared
{
    public interface IBunnyStreamService
    {
        Task<BunnyUploadCredentials> GenerateUrlToUploadVideoToBunnyStream(string title, string collectionIdentifier, CancellationToken cancellationToken = default);

        string GenerateUrlToAccessFileAsync(string videoGuid, CancellationToken cancellationToken = default);

        Task<BunnyCreateResponse> GetVideoDetails(String VideoId , string? LibraryId, CancellationToken cancellationToken);

        Task<bool> DeleteVideoFromBunnyStream(string videoGuid, CancellationToken cancellationToken);

        /// <summary>
        /// Delete multiple videos by collection ID
        /// </summary>
        Task<bool> DeleteVideosByCollectionIdAsync(string collectionId, CancellationToken cancellationToken);
    }

        // public required string VideoId { get; set; }
        // public string? LibraryId { get; set; }
}


