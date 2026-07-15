using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Model.Base
{
    /// <summary>
    /// Base class for entities that contain files (non-auditable)
    /// </summary>
    public abstract class BaseFileEntity : BaseEntity
    {
        /// <summary>
        /// The final accessible URL (only populated for Public files)
        /// </summary>
        public string? FileUrl { get; set; }

        /// <summary>
        /// The actual path in Bunny Storage (e.g. public/gyms/logo.jpg or private/receipts/abc123.pdf)
        /// </summary>
        public string StoredFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Determines if the file is publicly accessible without token
        /// </summary>
        public bool IsPublic { get; set; } = false;
    }
}