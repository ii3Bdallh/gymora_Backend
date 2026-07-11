using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base.Auditable
{
    public record BaseAuditableFUDTO : BaseAuditableUDTO
    {
        /// <summary>
        /// null = keep the old file. Provided = replace the old file (and delete it from Bunny).
        /// </summary>
        public virtual IFormFile? File { get; set; }
    }
}
