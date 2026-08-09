using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTO.Base
{
    public record BaseGymAuditableCDTO : BaseGymCDTO
    {
        public int CreatedByPersonId { get; set; }
    }

    public record BaseGymAuditableUDTO : BaseGymUDTO
    {
        public int CreatedByPersonId { get; set; }
    }
    public record BaseGymAuditableRDTO : BaseGymRDTO
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedByPersonId { get; set; }



    }

}