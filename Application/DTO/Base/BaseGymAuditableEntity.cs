using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTO.Base
{
    public record BaseGymAuditableCDTO : BaseCDTO
    {
        public int CreatedByPersonId { get; set; }
    }

    public record BaseGymAuditableUDTO : BaseUDTO
    {
        public int CreatedByPersonId { get; set; }
    }
    public record BaseGymAuditableRDTO : BaseRDTO
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedByPersonId { get; set; }



    }

}