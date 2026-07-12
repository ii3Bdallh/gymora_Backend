using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Base
{
    public record BaseFCDTO : BaseCDTO
    {
        [Required(ErrorMessage = "File is required.")]
        public virtual IFormFile File { get; set; } = null!;

        /// <summary>
        /// Controlled by the system - not set by client directly
        /// </summary>
        public bool IsPublic { get; set; } = false;
    }
}