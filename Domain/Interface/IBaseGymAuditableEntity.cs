using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Model;

namespace Domain.Interface
{
    public interface IBaseAuditableGymEntity
    {
        int CreatedByStaffId { get; set; }

        GymPerson CreatedByStaff { get; set; }
        DateTime CreatedOn { get; set; }

    }
}