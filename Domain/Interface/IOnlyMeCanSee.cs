using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interface
{
    public interface IOnlyMeCanSee
    {
        public int CreatedById { get; set; }
    }

    public interface IOnlyMeCanSeeAtGym
    {
        public int CreatedByPersonId { get; set; }
    }
}