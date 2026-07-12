namespace Domain.Options;

public sealed class BunnyOptions
{
    public const string SectionName = "Bunny";

    public BunnyStorageOptions BunnyStorageOptions { get; init; } = new BunnyStorageOptions();

    public BunnyStreamOptions BunnyStreamOptions { get; init; } = new BunnyStreamOptions();

    public PullZoneOptions PullZoneOptions { get; init; } = new PullZoneOptions();

}

public sealed class PullZoneOptions
{
    public const string SectionName = "PullZone";

    public string PullZoneUrl { get; init; } = string.Empty;

    public string CdnSignature { get; init; } = string.Empty;

    public int GenerateWatchUrlExpirationInMinutes { get; init; } = 5;

}



public sealed class BunnyStorageOptions
{
    public const string SectionName = "BunnyStorage";

    public string Password { get; init; } = string.Empty;

    public string StorageZoneRegionEndpoint { get; init; } = string.Empty;

    public int StorageMaxUploadSizeMB { get; init; } = 10;
}

public sealed class BunnyStreamOptions
{
    public const string SectionName = "BunnyStream";
    public string StreamApiKey { get; init; } = string.Empty;

    public string StreamSignature { get; init; } = string.Empty;


    public string LibraryId { get; init; } = string.Empty;


    public int UploadVideoExpirationInMinutes { get; init; } = 60;

    public int GenerateWatchUrlExpirationInMinutes { get; init; } = 5;
}


