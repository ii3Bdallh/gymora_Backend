namespace Application.Interface.Service.Shared
{
    /// <summary>
    /// Bunny Stream upload credentials response
    /// </summary>
    public class BunnyUploadCredentials
    {
        public string VideoId { get; set; } = string.Empty;
        public string LibraryId { get; set; } = string.Empty;
        public long ExpirationTime { get; set; }
        public string Signature { get; set; } = string.Empty;
    }
}


