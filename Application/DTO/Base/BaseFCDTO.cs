using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base
{
    public record BaseFCDTO : BaseCDTO
    {
        [Required(ErrorMessage = "File is required.")]
        public virtual IFormFile File { get; set; } = null!;
    }
}
