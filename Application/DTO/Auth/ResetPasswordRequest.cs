using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public record ResetPasswordRequest
    (
       [Required][EmailAddress] string Email,
    [Required][MaxLength(6)] string Otp,
    [Required][StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must be at least 8 characters with uppercase, lowercase, digit and special character")]
    string NewPassword
    );
}
