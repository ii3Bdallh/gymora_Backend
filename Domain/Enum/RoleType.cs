using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum RoleType
    {
        SuperAdmin = 0,
        User = 1,


    }





    public static class AppRole
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string User = "User";
    }

}
