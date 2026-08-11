namespace Domain.Interface
{
    public interface IBaseGymFileEntity : IBaseGymEntity
    {
        string? FileUrl { get; set; }
        string StoredFilePath { get; set; }
        bool IsPublic { get; set; }
    }
}
