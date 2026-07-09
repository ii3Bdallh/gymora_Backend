namespace Domain.Options;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public bool IsEnabled { get; init; } = true;

    public int DefaultAbsoluteExpirationMinutes { get; init; } = 10;

    public int DefaultSlidingExpirationMinutes { get; init; } = 5;
}
