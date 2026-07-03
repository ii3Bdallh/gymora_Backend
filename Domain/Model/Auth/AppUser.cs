using Microsoft.AspNetCore.Identity;
using Domain.Attributes;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model.Auth;

namespace Domain.Model
{
    [Searchable]
    public class AppUser : IdentityUser<int>
    {
        public string PersonName { get; set; } = string.Empty;

        // Password reset properties
        public string? PasswordResetOtp { get; set; }

        public DateTime? PasswordResetOtpExpiry { get; set; }

        public int PasswordResetOtpAttempts { get; set; } = 0;

        // Email Confirmation properties
        public string? EmailConfirmationOtp { get; set; }
        public DateTime? EmailConfirmationOtpExpiry { get; set; }
        public int EmailConfirmationOtpAttempts { get; set; } = 0;


        // Collections
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();


        public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();



    }
}

