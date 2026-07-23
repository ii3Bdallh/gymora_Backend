using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTO.Base
{
    public record BaseGymAuditableCDTO : BaseCDTO
    {
        public int CreatedByStaffId { get; set; }
    }

    public record BaseGymAuditableUDTO : BaseUDTO
    {
        public int CreatedByStaffId { get; set; }
    }
    public record BaseGymAuditableRDTO : BaseRDTO
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedByStaffId { get; set; }



    }

}