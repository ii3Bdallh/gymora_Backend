using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Request
{

    public record ConfirmEmailRequest(
        [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

        [Required(ErrorMessage = "OTP is required")]
    [StringLength(5, MinimumLength = 5, ErrorMessage = "OTP must be 5 digits")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "OTP must contain only digits")]
    string Otp
    );

    public record ResendConfirmationRequest(
        [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email
    );



    public record ChangePasswordRequest(
        [Required(ErrorMessage = "Current password is required")]
    string CurrentPassword,

        [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    string NewPassword
    );
}