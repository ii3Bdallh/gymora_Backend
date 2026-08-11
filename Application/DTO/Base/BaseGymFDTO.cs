using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Application.DTO.Base
{
    public record BaseGymFCDTO : BaseGymCDTO
    {
        public virtual IFormFile? File { get; set; }
        public bool IsPublic { get; set; }
    }

    public record BaseGymFUDTO : BaseGymUDTO
    {
        public virtual IFormFile? File { get; set; }
    }

    public record BaseGymFRDTO : BaseGymRDTO
    {
        public string FileUrl { get; set; } = string.Empty;
    }
}
