using System;
using Domain.Model.Auth;
using Domain.Model.Base;

namespace Domain.Model
{
    public class UserWorkoutBlock : BaseEntity
    {
        public int BlockedUserId { get; set; }
        public ApplicationUser BlockedUser { get; set; } = null!;

        public DateTime BlockedUntil { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
