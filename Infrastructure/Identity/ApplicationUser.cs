using Domain.Interface;
using Domain.Model;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<int>, IUser
{
    public string PersonName { get; set; } = string.Empty;

    public string? PasswordResetOtp { get; set; }

    public DateTime? PasswordResetOtpExpiry { get; set; }

    public int PasswordResetOtpAttempts { get; set; }

    public string? EmailConfirmationOtp { get; set; }

    public DateTime? EmailConfirmationOtpExpiry { get; set; }

    public int EmailConfirmationOtpAttempts { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
