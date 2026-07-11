using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base.Auditable
{
    public record BaseAuditableFCDTO : BaseAuditableCDTO
    {
        [Required(ErrorMessage = "File is required.")]
        public virtual IFormFile File { get; set; } = null!;
    }
}
