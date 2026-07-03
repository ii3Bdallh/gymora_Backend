using Application.Interface.Service.Shared;
using Application.utils;
using Domain.Model;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Model.Json;

namespace Application.Service.Shared
{
    public class BunnyStorageService(BunnyConfig bunnyConfig, HttpClient httpClient, ILogger<BunnyStorageService> logger) : IBunnyStorageService
    {


        /// <summary>
        /// Upload a file to Bunny Storage
        /// </summary>
        public async Task<string> UploadFileToBunnyStorageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            // Validate file is not null
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            // Validate file size against max upload size
            var maxSizeBytes = bunnyConfig.StorageMaxUploadSizeMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
                throw new ArgumentException($"File size exceeds maximum allowed size of {bunnyConfig.StorageMaxUploadSizeMB}MB. Current file size: {Math.Round(file.Length / (1024.0 * 1024.0), 2)}MB");

            // Generate unique filename
            var finalFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var request = new HttpRequestMessage(HttpMethod.Put, $"https://storage.bunnycdn.com/{bunnyConfig.StorageName}/{finalFileName}");
            request.Headers.Add("AccessKey", bunnyConfig.StorageApiKey);

            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(file.ContentType ?? "application/octet-stream");



            request.Content = content;

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation($"File uploaded successfully to Bunny Storage: {finalFileName}");
                return finalFileName;
            }

            throw new Exception($"Failed to upload file Please try again.");
        }

        /// <summary>
        /// Generate secure access URL for a file from Bunny Storage
        /// </summary>
        public string GenerateUrlToAccessFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var secureUrl = GenerateSecureUrlForBasicCdn(fileName);
            return secureUrl;
        }

        /// <summary>
        /// Generates a secure CDN URL with MD5-based token authentication
        /// Per BunnyCDN documentation: token = Base64(MD5(security_key + path + expiration))
        /// </summary>
        private string GenerateSecureUrlForBasicCdn(string fileName, string? ipAddress = null)
        {
            string securityKey = bunnyConfig.CDNSignature;
            string pullZoneUrl = bunnyConfig.PullZoneUrl;
            string storageName = bunnyConfig.StorageName;
            long expiresUnix = DateTimeOffset.UtcNow.AddMinutes(bunnyConfig.GenerateWatchUrlExpirationInMinutes).ToUnixTimeSeconds();

            // Normalize path - ensure it starts with /
            string filePath = fileName.StartsWith("/") ? fileName : $"/{fileName}";

            // Build hashable string per BunnyCDN spec: security_key + path + expiration
            string hashableString = securityKey + filePath + expiresUnix + ipAddress;

            // Generate MD5 token using HashingService
            string token = HashingService.GenerateMD5Token(hashableString);

            // Construct the secure URL
             string secureUrl = $"{pullZoneUrl}{filePath}?token={token}&expires={expiresUnix}";

            logger.LogInformation("Generated secure CDN URL for file: {FileName} with expiration timestamp: {Expires}",
                fileName, expiresUnix);

            return secureUrl;
        }

        public async Task<bool> DeleteFileFromBunnyStorageAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://storage.bunnycdn.com/{bunnyConfig.StorageName}/{fileName}");
            request.Headers.Add("AccessKey", bunnyConfig.StorageApiKey);

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("File deleted successfully from Bunny Storage: {FileName}", fileName);
                return true;
            }

            logger.LogError("Failed to delete file from Bunny Storage: {FileName}, Status: {Status}", fileName, response.StatusCode);
            return false;
        }

        public Task<bool> DeleteCollectionFromBunnyStorageAsync(string collectionPath, CancellationToken cancellationToken = default)
        {
            return DeleteFileFromBunnyStorageAsync(collectionPath, cancellationToken);
        }
    }


}
