using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Model.Base;

namespace Domain.Interface
{
    public class BaseAuditableGymEntity : BaseGymEntity, IBaseAuditableGymEntity
    {
        public int CreatedByStaffId { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}