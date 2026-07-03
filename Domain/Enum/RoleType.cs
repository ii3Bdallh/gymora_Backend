using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum RoleType
    {
        Owner = 0,
        Admin = 1,
        User = 2,
        Guest = 3,

        Marchant = 4,
        Parent = 5

    }

  

        public static class RolesEnumExtensions
        {
            public static string ToRoleString(this RoleType role)
            {
                return role.ToString(); // في حالة لو حابب تغيرها مستقبلاً (مثلاً role.ToString().ToLower())
            }
    }

}
