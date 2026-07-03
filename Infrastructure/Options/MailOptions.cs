namespace Infrastructure.Options;

public sealed class MailOptions
{
    public const string SectionName = "Mail";

    public string FromEmail { get; init; } = string.Empty;

    public string FromPassword { get; init; } = string.Empty;
}
