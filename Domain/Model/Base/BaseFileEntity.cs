using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Model.Base
{
    public abstract class BaseFileEntity : BaseEntity
    {
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// The name/path the file is actually stored under in Bunny Storage.
        /// </summary>
        public string StoredFileName { get; set; } = string.Empty;
    }
}
