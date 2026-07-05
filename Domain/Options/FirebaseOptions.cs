namespace Domain.Options;

public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string CredentialFilePath { get; init; } = string.Empty;
}
