using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base
{
    public record BaseFUDTO : BaseUDTO
    {
        /// <summary>
        /// null = keep old file | Provided = replace file
        /// </summary>
        public virtual IFormFile? File { get; set; }

        public bool IsPublic { get; set; } = false;
    }
}