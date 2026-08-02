using Domain.Model;
using Domain.Model.Base;
using Microsoft.AspNetCore.Identity;

namespace Domain.Model.Auth;

public sealed class ApplicationUser : IdentityUser<int>
{
    public string PersonName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;


    public string? PasswordResetOtp { get; set; }

    public DateTime? PasswordResetOtpExpiry { get; set; }

    public int PasswordResetOtpAttempts { get; set; }

    public string? EmailConfirmationOtp { get; set; }

    public DateTime? EmailConfirmationOtpExpiry { get; set; }

    public int EmailConfirmationOtpAttempts { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
