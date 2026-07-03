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

  

        public static class RolesEnumExtensions
        {
            public static string ToRoleString(this RoleType role)
            {
                return role.ToString(); // في حالة لو حابب تغيرها مستقبلاً (مثلاً role.ToString().ToLower())
            }
    }

}
