using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum GymRoleType
    {
        Owner = 0,
        Coach = 1,
        Trainee = 2,
        Trainer = 3,


    }



    public static class GymRolesEnumExtensions
    {
        public static string ToRoleString(this GymRoleType role)
        {
            return role.ToString(); // في حالة لو حابب تغيرها مستقبلاً (مثلاً role.ToString().ToLower())
        }
    }

}
