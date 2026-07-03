using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Auth
{
    public record GetUserProfileDto(string? Email , string? PersonName , string? PhoneNumber , IList<string>? Roles);

}
