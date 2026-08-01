using System;
using System.Collections.Generic;

namespace Application.DTO.Auth
{
    public class GetUserProfileDto
    {
        public string? Email { get; set; }
        public string? PersonName { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<string>? Roles { get; set; }

        public GetUserProfileDto() { }

        public GetUserProfileDto(string? email, string? personName, string? phoneNumber, IList<string>? roles)
        {
            Email = email;
            PersonName = personName;
            PhoneNumber = phoneNumber;
            Roles = roles;
        }
    }
}
