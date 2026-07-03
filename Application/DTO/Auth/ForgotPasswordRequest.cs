using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public record ForgotPasswordRequest
    (
        [Required]
        [EmailAddress]
        string Email 
   
   );
}




