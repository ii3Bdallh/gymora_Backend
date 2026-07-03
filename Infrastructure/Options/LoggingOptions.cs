namespace Infrastructure.Options;

public sealed class LoggingOptions
{
    public const string SectionName = "Logging";

    public string Path { get; init; } = "logs/log-.txt";

    public string MinimumLevel { get; init; } = "Information";

    public bool EnableConsole { get; init; } = true;

    public bool EnableFile { get; init; } = true;
}
