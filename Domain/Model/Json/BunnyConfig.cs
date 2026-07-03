namespace Domain.Model.Json
{
    public class BunnyConfig
    {
        public string StreamApiKey { get; set; } = string.Empty;
        public string LibraryId { get; set; } = string.Empty;
        public string CdnHostName { get; set; } = string.Empty;
        public string PullZoneUrl { get; set; } = string.Empty;
        public string StorageApiKey { get; set; } = string.Empty;
        public string StreamSignature { get; set; } = string.Empty;
        public string CDNSignature { get; set; } = string.Empty;
        public string StorageName { get; set; } = string.Empty;
        public int StorageMaxUploadSizeMB { get; set; } = 100;
        public int UploadVideoExpirationInMinutes { get; set; } = 60;
        public int UploadFileExpirationInMinutes { get; set; } = 60;
        public int GenerateWatchUrlExpirationInMinutes { get; set; } = 5;

        public string BunnyCdnBaseUrl { get; set; } = string.Empty ;
    }
}