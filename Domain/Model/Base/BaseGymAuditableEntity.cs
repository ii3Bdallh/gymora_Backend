using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Model.Base;
using Domain.Interface;

namespace Domain.Model.Base
{
    public class BaseAuditableGymEntity : BaseGymEntity, IBaseAuditableGymEntity
    {
        public int CreatedByStaffId { get; set; }

        public GymStaff CreatedByStaff { get; set; } = default!;
        public DateTime CreatedOn { get; set; }

    }
}