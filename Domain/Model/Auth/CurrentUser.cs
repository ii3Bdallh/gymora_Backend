using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Model.Auth
{
    public class CurrentUser
    {
        public int UserId { get; set; }
        public bool IsAdmin { get; set; }

        public bool IsNotAdmin => !IsAdmin;

    }


}