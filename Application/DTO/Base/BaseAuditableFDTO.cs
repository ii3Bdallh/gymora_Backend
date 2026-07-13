using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base
{
    public record BaseAuditableFCDTO : BaseAuditableCDTO
    {
        public virtual IFormFile? File { get; set; }
        public bool IsPublic { get; set; } = false;
    }
    public record BaseAuditableFUDTO : BaseAuditableUDTO
    {
        public virtual IFormFile? File { get; set; }
    }
    public record BaseAuditableFRDTO : BaseAuditableRDTO
    {
        public string FileUrl { get; set; } = string.Empty;

    }
}