using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Base
{
    public record BaseAuditableFCDTO : BaseFCDTO
    {


        public int CreatedById { get; set; }

    }
    public record BaseAuditableFUDTO : BaseFUDTO
    {
        public int CreatedById { get; set; }

    }
    public record BaseAuditableFRDTO : BaseFRDTO
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }
        public DateTime? ModifiedOn { get; set; }

    }


}
