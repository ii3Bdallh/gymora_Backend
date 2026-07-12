using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base.Auditable
{
    public record BaseAuditableFUDTO : BaseAuditableUDTO
    {
        public virtual IFormFile? File { get; set; }
    }
}
