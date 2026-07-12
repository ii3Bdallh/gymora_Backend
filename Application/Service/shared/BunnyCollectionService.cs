using Application.DTO.Bunny;
using Application.Interface.Service.Shared;
using Application.utils;
using Domain.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Shared
{
    /// <summary>
    /// Service for managing Bunny Stream Collections (CRUD operations)
    /// </summary>
    public class BunnyCollectionService(BunnyOptions settings, HttpClient httpClient, ILogger<BunnyCollectionService> logger) : IBunnyCollectionService
    {

        // {{baseUrl}}/library/:libraryId/collections
        public async Task<BunnyCollectionDTO> GetCollectionAsync(string collectionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionId))
                throw new ArgumentException("Collection ID cannot be empty", nameof(collectionId));

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://video.bunnycdn.com/library/{settings.BunnyStreamOptions.LibraryId}/collections/{collectionId}");
            request.Headers.Add("AccessKey", settings.BunnyStreamOptions.StreamApiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError($"Bunny API error getting collection: StatusCode={response.StatusCode}, Content={errorContent}");
                throw new Exception($"Failed to get collection from Bunny Stream: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<BunnyCollectionDTO>(cancellationToken: cancellationToken);
            if (result == null)
                throw new Exception("Invalid response from Bunny API - collection data is null");

            logger.LogInformation($"Collection retrieved successfully: {result.Guid}");
            return result;
        }

        /// <summary>
        /// Create a new collection
        /// </summary>
        /// {{baseUrl}}/library/:libraryId/collections
        public async Task<BunnyCollectionDTO> CreateCollectionAsync(CreateBunnyCollectionDTO dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Collection name cannot be empty", nameof(dto.Name));

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://video.bunnycdn.com/library/{settings.BunnyStreamOptions.LibraryId}/collections");
            request.Headers.Add("AccessKey", settings.BunnyStreamOptions.StreamApiKey);
            request.Content = JsonContent.Create(new { name = dto.Name });

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError($"Bunny API error creating collection: StatusCode={response.StatusCode}, Content={errorContent}");
                throw new Exception($"Failed to create collection on Bunny Stream: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<BunnyCollectionDTO>(cancellationToken: cancellationToken);
            if (result == null || string.IsNullOrEmpty(result.Guid))
                throw new Exception("Invalid response from Bunny API - missing collection Guid");

            logger.LogInformation($"Collection created successfully: {result.Guid} - {result.Name}");
            return result;
        }

        /// <summary>
        /// Update an existing collection
        /// </summary>
        public async Task UpdateCollectionAsync(string collectionId, UpdateBunnyCollectionDTO dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionId))
                throw new ArgumentException("Collection ID cannot be empty", nameof(collectionId));

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Collection name cannot be empty", nameof(dto.Name));

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://video.bunnycdn.com/library/{settings.BunnyStreamOptions.LibraryId}/collections/{collectionId}");
            request.Headers.Add("AccessKey", settings.BunnyStreamOptions.StreamApiKey);
            request.Content = JsonContent.Create(new { name = dto.Name });

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError($"Bunny API error updating collection: StatusCode={response.StatusCode}, Content={errorContent}");
                throw new Exception($"Failed to update collection on Bunny Stream: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<BunnyCollectionDTO>(cancellationToken: cancellationToken);
            if (result == null)
                throw new Exception("Invalid response from Bunny API - updated collection data is null");

            logger.LogInformation($"Collection updated successfully: {result.Guid} - {result.Name}");
            return ;
        }

        /// <summary>
        /// Delete a collection
        /// </summary>
        public async Task<bool> DeleteCollectionAsync(string collectionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionId))
                throw new ArgumentException("Collection ID cannot be empty", nameof(collectionId));

            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://video.bunnycdn.com/library/{settings.BunnyStreamOptions.LibraryId}/collections/{collectionId}");
            request.Headers.Add("AccessKey", settings.BunnyStreamOptions.StreamApiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError($"Bunny API error deleting collection: StatusCode={response.StatusCode}, Content={errorContent}");
                throw new Exception($"Failed to delete collection from Bunny Stream: {response.StatusCode}");
            }

            logger.LogInformation($"Collection deleted successfully: {collectionId}");
            return true;
        }
    }
}
