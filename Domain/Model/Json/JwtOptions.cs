using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Json
{
    public class JwtOptions
    {


        public string issuer { get; set; } = String.Empty;
        public string audience { get; set; } = String.Empty;
        public string SecretKey { get; set; } = String.Empty;
        public int TokenExpirationInMinutes { get; set; }
        public int RefreshTokenExpirationInDays { get; set; } = 7;

    }
}

