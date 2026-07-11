using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base
{
    public record BaseFUDTO : BaseUDTO
    {
        /// <summary>
        /// null = keep the old file. Provided = replace the old file (and delete it from Bunny).
        /// </summary>
        public virtual IFormFile? File { get; set; }
    }
}
