namespace Infrastructure.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;

    public string InstanceName { get; init; } = string.Empty;
}
