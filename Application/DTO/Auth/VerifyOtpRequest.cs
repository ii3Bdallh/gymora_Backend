using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Auth
{
    public record VerifyOtpRequest(
        [Required]
        [EmailAddress]
        string Email,
        [Required]
        [MaxLength(6)]
        string Otp
    );

}
