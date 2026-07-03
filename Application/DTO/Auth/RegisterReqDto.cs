using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public record RegisterReqDto(
    [EmailAddress] string Email,
    [StringLength(100, MinimumLength = 3)] string UserName,
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must be at least 8 characters with uppercase, lowercase, digit and special character")]
    string Password,
    [StringLength(50)] string PhoneNumber
);
}
