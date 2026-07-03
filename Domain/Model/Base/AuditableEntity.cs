
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Base
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public AppUser? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
        


    }
   
   
}
