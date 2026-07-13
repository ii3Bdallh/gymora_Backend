using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base
{
    public record BaseFCDTO : BaseCDTO
    {
        public virtual IFormFile? File { get; set; }

        public bool IsPublic { get; set; }
    }

    public record BaseFRDTO : BaseRDTO
    {
        public string FileUrl { get; set; } = string.Empty;
    }
    public record BaseFUDTO : BaseUDTO
    {

        public virtual IFormFile? File { get; set; }

    }
}