using Domain.Interface;

namespace Domain.Model.Base
{
    public abstract class BaseGymFileEntity : BaseGymEntity, IBaseFileEntity
    {
        public string? FileUrl { get; set; }
        public string StoredFilePath { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
    }
}
