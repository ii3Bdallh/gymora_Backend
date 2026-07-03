namespace Infrastructure.Options;

public sealed class BunnyOptions
{
    public const string SectionName = "Bunny";

    public string LibraryId { get; init; } = string.Empty;

    public string CdnHostName { get; init; } = string.Empty;

    public string StreamApiKey { get; init; } = string.Empty;

    public string StorageApiKey { get; init; } = string.Empty;

    public string PullZoneUrl { get; init; } = string.Empty;

    public string StorageName { get; init; } = string.Empty;

    public string StreamSignature { get; init; } = string.Empty;

    public string CdnSignature { get; init; } = string.Empty;

    public int StorageMaxUploadSizeMB { get; init; } = 10;

    public int UploadVideoExpirationInMinutes { get; init; } = 60;

    public int GenerateWatchUrlExpirationInMinutes { get; init; } = 5;

    public int UploadFileExpirationInMinutes { get; init; } = 60;

    public string BunnyCdnBaseUrl { get; init; } = string.Empty;
}
