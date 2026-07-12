using Application.DTO;
using Application.DTO.Bunny;
using Application.Interface.Service.Shared;
using Application.utils;
using Domain.Model;
using Domain.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Shared
{
    public class BunnyStreamService(BunnyOptions settings, HttpClient httpClient, ILogger<BunnyStreamService> logger) : IBunnyStreamService
    {

        public async Task<BunnyCreateResponse> GetVideoDetails(String VideoId , string? LibraryId, CancellationToken cancellationToken)
        {
            LibraryId = LibraryId ?? settings.BunnyStreamOptions.LibraryId;
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://video.bunnycdn.com/library/{LibraryId}/videos/{VideoId}");
            request.Headers.Add("AccessKey", settings.BunnyStreamOptions.StreamApiKey);


            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError($"Bunny API error: {response.StatusCode} - {errorContent}");
                throw new Exception("Failed to create video on Bunny Stream");
            }

            var bunnyResult = await response.Content.ReadFromJsonAsync<BunnyCreateResponse>(cancellationToken: cancellationToken);

            if (bunnyResult == null || string.IsNullOrEmpty(bunnyResult.Guid))
            {
                logger.LogError("Invalid response from Bunny API - missing Guid");
                throw new Exception("Invalid response from Bunny API - missing Guid");
            }

            logger.LogInformation($"Video created successfully on Bunny: {bunnyResult.Guid}");
            return bunnyResult;
        }

        /// <summary>
        /// Generate upload credentials for video to Bunny Stream
        /// </summary>
        public async Task<BunnyUploadCredentials> GenerateUrlToUploadVideoToBunnyStream(string title, string collectionIdentifier, CancellationToken cancellationToken = default)
        {

            var videoData = await CreateVideoAsync(title, collectionIdentifier, cancellationToken);
            if (videoData == null)
                return new BunnyUploadCredentials { VideoId = "", LibraryId = "", ExpirationTime = 0, Signature = "" };

            long expirationTime = DateTimeOffset.UtcNow.AddMinutes(settings.BunnyStreamOptions.UploadVideoExpirationInMinutes).ToUnixTimeSeconds();
            string signatureString = settings.BunnyStreamOptions.LibraryId + settings.BunnyStreamOptions.StreamApiKey + expirationTime + videoData.Guid;
            string signature = HashingService.GenerateSHA256(signatureString);

            return new BunnyUploadCredentials()
            {
                VideoId = videoData.Guid,
                LibraryId = settings.BunnyStreamOptions.LibraryId,
                ExpirationTime = expirationTime,
                Signature = signature
            };

        }

        /// <summary>
        /// Generate secure access URL for video from Bunny Stream
        /// </summary>
        public string GenerateUrlToAccessFileAsync(string videoGuid, CancellationToken cancellationToken = default)
        {

            var videoUrl = GenerateSecureUrlForEmbed(videoGuid);
            return videoUrl;

        }

        /// <summary>
        /// Create a video object on Bunny Stream API
        /// </summary>
        private async Task<BunnyCreateResponse> CreateVideoAsync(string videoTitle, string collectionIdentifier, CancellationToken cancellationToken)
        {
            videoTitle = videoTitle + "_" + Guid.NewGuid().ToString(); // Ensure unique title to avoid conflicts in Bunny Stream
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://video.bunnycdn.com/library/{settings.BunnyStreamOptions.LibraryId}/videos");
            request.Headers.Add("AccessKey", settings.BunnyStreamOptions.StreamApiKey);
            request.Content = JsonContent.Create(new { title = videoTitle, collectionId = collectionIdentifier });

            var response = await httpClient.SendAsync(request, cancellationToken);



            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError($"Bunny API error: {response.StatusCode} - {errorContent}");
                throw new Exception("Failed to create video on Bunny Stream");
            }

            var bunnyResult = await response.Content.ReadFromJsonAsync<BunnyCreateResponse>(cancellationToken: cancellationToken);

            if (bunnyResult == null || string.IsNullOrEmpty(bunnyResult.Guid))
            {
                logger.LogError("Invalid response from Bunny API - missing Guid");
                throw new Exception("Invalid response from Bunny API - missing Guid");
            }

            logger.LogInformation($"Video created successfully on Bunny: {bunnyResult.Guid}");
            return bunnyResult;

        }

        /// <summary>
        /// Generates a secure iframe embed URL with token and expiration
        /// </summary>
        private string GenerateSecureUrlForEmbed(string videoId)
        {
            string securityKey = settings.BunnyStreamOptions.StreamSignature;
            string libraryId = settings.BunnyStreamOptions.LibraryId;
            long expires = DateTimeOffset.UtcNow.AddMinutes(settings.BunnyStreamOptions.GenerateWatchUrlExpirationInMinutes).ToUnixTimeSeconds();

            string stringToHash = securityKey + videoId + expires;
            string token = HashingService.GenerateSHA256(stringToHash);

            return $"https://iframe.mediadelivery.net/embed/{libraryId}/{videoId}?token={token}&expires={expires}";
        }

        public async Task<bool> DeleteVideoFromBunnyStream(string videoGuid, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(videoGuid))
            {
                logger.LogWarning("DeleteVideoFromBunnyStream called with empty videoGuid");
                return false;
            }

            logger.LogInformation($"Deleting video from Bunny Stream: {videoGuid}");
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://video.bunnycdn.com/library/{settings.BunnyStreamOptions.LibraryId}/videos/{videoGuid}");

            request.Headers.Add("AccessKey", settings.BunnyStreamOptions.StreamApiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation($"Video deleted successfully from Bunny Stream: {videoGuid}");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError($"Failed to delete video from Bunny Stream. Status: {response.StatusCode} - {errorContent}");
                return false;
            }
        }


        public Task<bool> DeleteVideosByCollectionIdAsync(string collectionId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }


}


