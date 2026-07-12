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
using Domain.Options;


namespace Application.Service.Shared
{
    public class BunnyStorageService(BunnyOptions bunnyConfig, HttpClient httpClient, ILogger<BunnyStorageService> logger)
        : IStorageService
    {
        // StorageName
        // StorageApiKey
        // CDnSignature
        // PullZoneUrl
        
        /// <summary>
        /// Upload a file to Bunny Storage
        /// </summary>
        public async Task<string> UploadFileToStorageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            var maxSizeBytes = bunnyConfig.BunnyStorageOptions.StorageMaxUploadSizeMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
                throw new ArgumentException($"File size exceeds maximum allowed size of {bunnyConfig.BunnyStorageOptions.StorageMaxUploadSizeMB}MB. Current file size: {Math.Round(file.Length / (1024.0 * 1024.0), 2)}MB");

            var finalFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var request = new HttpRequestMessage(HttpMethod.Put, $"{bunnyConfig.BunnyStorageOptions.StorageZoneRegionEndpoint}/{finalFileName}");
            request.Headers.Add("AccessKey", bunnyConfig.BunnyStorageOptions.Password);

            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(file.ContentType ?? "application/octet-stream");
            request.Content = content;

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("File uploaded successfully to Bunny Storage: {FileName}", finalFileName);
                return finalFileName;
            }

            throw new Exception("Failed to upload file. Please try again.");
        }

        /// <summary>
        /// Generate secure access URL for a file from Bunny Storage
        /// </summary>
        public string GenerateUrlToAccessFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return GenerateSecureUrlForBasicCdn(fileName);
        }

        /// <summary>
        /// Generates a secure CDN URL with MD5-based token authentication.
        /// Per BunnyCDN documentation: token = Base64(MD5(security_key + path + expiration))
        /// </summary>
        private string GenerateSecureUrlForBasicCdn(string fileName, string? ipAddress = null)
        {
            string securityKey = bunnyConfig.PullZoneOptions.CdnSignature;
            string pullZoneUrl = bunnyConfig.PullZoneOptions.PullZoneUrl;
            long expiresUnix = DateTimeOffset.UtcNow.AddMinutes(bunnyConfig.PullZoneOptions.GenerateWatchUrlExpirationInMinutes).ToUnixTimeSeconds();

            string filePath = fileName.StartsWith("/") ? fileName : $"/{fileName}";

            string hashableString = securityKey + filePath + expiresUnix + ipAddress;
            string token = HashingService.GenerateMD5Token(hashableString);

            string secureUrl = $"{pullZoneUrl}{filePath}?token={token}&expires={expiresUnix}";

            logger.LogInformation("Generated secure CDN URL for file: {FileName} with expiration timestamp: {Expires}",
                fileName, expiresUnix);

            return secureUrl;
        }

        public async Task<bool> DeleteFileFromStorageAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{bunnyConfig.BunnyStorageOptions.StorageZoneRegionEndpoint}/{fileName}");
            request.Headers.Add("AccessKey", bunnyConfig.BunnyStorageOptions.Password);

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("File deleted successfully from Bunny Storage: {FileName}", fileName);
                return true;
            }

            logger.LogError("Failed to delete file from Bunny Storage: {FileName}, Status: {Status}", fileName, response.StatusCode);
            return false;
        }

        public Task<bool> DeleteCollectionFromStorageAsync(string collectionPath, CancellationToken cancellationToken = default)
        {
            // Bunny Storage API مفيهاش endpoint لمسح فولدر كامل دفعة واحدة بنفس منطق الملف الواحد،
            // فهنا لو محتاج فعليًا تمسح فولدر لازم تستخدم List + Loop زي اللي عملناه في Firebase تحت.
            return DeleteFileFromStorageAsync(collectionPath, cancellationToken);
        }
    }
}


