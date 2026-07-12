using Application.Interface.Service.Shared;
using Application.utils;
using Domain.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Application.Service.Shared
{
    public class BunnyStorageService : IStorageService
    {
        private readonly BunnyOptions _bunny;
        private readonly HttpClient _httpClient;
        private readonly ILogger<BunnyStorageService> _logger;

        public BunnyStorageService(
            BunnyOptions bunnyConfig,
            HttpClient httpClient,
            ILogger<BunnyStorageService> logger)
        {
            _bunny = bunnyConfig;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> UploadFileToStorageAsync(
            IFormFile file,
            bool isPublic,
            string entityType,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            // التحقق من الحجم
            var maxSize = _bunny.BunnyStorageOptions.StorageMaxUploadSizeMB * 1024 * 1024;
            if (file.Length > maxSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {_bunny.BunnyStorageOptions.StorageMaxUploadSizeMB}MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";

            // تحديد المجلد بشكل آمن (لا نسمح للـ DTO بتحديد المسار)
            string folder = isPublic ? "public" : "private";
            string subFolder = entityType.ToLowerInvariant() switch
            {
                "gym" or "gyms" => "gyms",
                "exercise" or "exercises" => "exercises",
                "member" or "members" => "members",
                "receipt" or "payment" or "proof" => "receipts",
                "logo" => "logos",
                _ => "others"
            };

            var storagePath = $"{folder}/{subFolder}/{fileName}";

            // رفع الملف
            var request = new HttpRequestMessage(HttpMethod.Put,
                $"{_bunny.BunnyStorageOptions.StorageZoneRegionEndpoint}/{storagePath}");

            request.Headers.Add("AccessKey", _bunny.BunnyStorageOptions.Password);

            using var stream = file.OpenReadStream();
            request.Content = new StreamContent(stream)
            {
                Headers = { ContentType = MediaTypeHeaderValue.Parse(file.ContentType ?? "application/octet-stream") }
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to upload to Bunny: {Status} - {Error}", response.StatusCode, error);
                throw new Exception("Failed to upload file to storage.");
            }

            _logger.LogInformation("File uploaded successfully: {Path} (Public: {IsPublic})", storagePath, isPublic);
            return storagePath;
        }

        public string GetFileAccessUrl(string storedFileName, bool isPublic)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
                return string.Empty;

            if (isPublic)
            {
                // رابط مباشر دائم
                return $"{_bunny.PullZoneOptions.PullZoneUrl}/{storedFileName}";
            }
            else
            {
                // Signed URL جديد
                return GenerateSecureSignedUrl(storedFileName);
            }
        }

        private string GenerateSecureSignedUrl(string filePath)
        {
            string securityKey = _bunny.PullZoneOptions.CdnSignature;
            string pullZoneUrl = _bunny.PullZoneOptions.PullZoneUrl;
            long expires = DateTimeOffset.UtcNow
                .AddMinutes(_bunny.PullZoneOptions.GenerateWatchUrlExpirationInMinutes)
                .ToUnixTimeSeconds();

            string path = filePath.StartsWith("/") ? filePath : $"/{filePath}";
            string hashable = securityKey + path + expires;
            string token = HashingService.GenerateMD5Token(hashable);

            return $"{pullZoneUrl}{path}?token={token}&expires={expires}";
        }

        public async Task<bool> DeleteFileFromStorageAsync(string storedFileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"{_bunny.BunnyStorageOptions.StorageZoneRegionEndpoint}/{storedFileName}");

            request.Headers.Add("AccessKey", _bunny.BunnyStorageOptions.Password);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("File deleted: {Path}", storedFileName);
                return true;
            }

            _logger.LogWarning("Failed to delete file: {Path} - Status: {Status}", storedFileName, response.StatusCode);
            return false;
        }

        public Task<bool> DeleteCollectionFromStorageAsync(string collectionPath, CancellationToken cancellationToken = default)
        {
            // حالياً نحذف ملف واحد فقط، يمكن توسيعه لاحقاً
            return DeleteFileFromStorageAsync(collectionPath, cancellationToken);
        }
    }
}