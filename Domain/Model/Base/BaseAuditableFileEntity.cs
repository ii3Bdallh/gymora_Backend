namespace Domain.Model.Base
{
    /// <summary>
    /// Base class for auditable entities that contain files
    /// </summary>
    public abstract class BaseAuditableFileEntity : AuditableEntity
    {
        public string? FileUrl { get; set; }
        public string StoredFileName { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
    }
}