using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Model.Json
{
    public class EmailOptions
    {
        public String FromEmail { get; set; } = String.Empty;
        public String FromPassword { get; set; } = String.Empty;  
    }
}