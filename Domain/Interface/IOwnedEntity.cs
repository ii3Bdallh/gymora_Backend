using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interface
{
    public interface IOwnedEntity
    {
        public int CreatedById { get; set; }
    }

    public interface ICacheTT
    {
        
    }
}