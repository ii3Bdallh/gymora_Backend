namespace Domain.Options
{
    public class FirebaseStorageOptions
    {
        public const string SectionName = "FirebaseStorage";
        public String BucketName { get; init; } = string.Empty;
    }
}