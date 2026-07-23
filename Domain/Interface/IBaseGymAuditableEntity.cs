using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interface
{
    public interface IBaseAuditableGymEntity
    {
        int CreatedByStaffId { get; set; }

        DateTime CreatedOn { get; set; }

    }
}