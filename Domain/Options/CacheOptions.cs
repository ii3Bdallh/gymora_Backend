namespace Domain.Options;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";


    public int DefaultAbsoluteExpirationMinutes { get; init; } = 10;

    public int DefaultSlidingExpirationMinutes { get; init; } = 5;
}
