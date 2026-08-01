using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Auth
{
    public class ConfirmEmailRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "OTP must be 5 digits")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "OTP must contain only digits")]
        public string Otp { get; set; } = null!;
    }

    public class ResendConfirmationRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = null!;
    }
}