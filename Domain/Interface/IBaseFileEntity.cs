namespace Domain.Interface
{
    public interface IBaseFileEntity : IBaseEntity
    {
        string? FileUrl { get; set; }
        string StoredFilePath { get; set; }
        bool IsPublic { get; set; }
    }
}
