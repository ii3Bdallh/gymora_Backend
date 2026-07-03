using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }


        public string Token { get; set; } = String.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpirationAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpirationAt;
        public bool IsValid => RevokedAt is null && !IsExpired;

        public int UserId { get; set; }

        public AppUser User { get; set; } = null!; // Navigation property to AppUser


    }
}

