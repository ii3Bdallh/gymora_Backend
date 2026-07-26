using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO.Model;

namespace Application.DTO
{
    public record LoginResDto
    (
        int Id,
        string Email,
        string PersonName,
        string Token,
        int ExpiresIn,
        string Refreshtoken,
        IEnumerable<string> Roles,
        DateTime RefreshTokenExpirationDate,
        MyGymDto? MyGym
);
}

