using System;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record UserWorkoutBlockCDTO : BaseCDTO
    {
        [Required(ErrorMessage = "Blocked user ID is required")]
        public int BlockedUserId { get; set; }

        [Required(ErrorMessage = "Duration in days is required")]
        public int DurationDays { get; set; } // 1 for day, 30 for month, -1 or 9999 for lifetime

        public string? Reason { get; set; }
    }

    public record UserWorkoutBlockUDTO : BaseUDTO
    {
        [Required(ErrorMessage = "Blocked until date is required")]
        public DateTime BlockedUntil { get; set; }

        public string? Reason { get; set; }
    }

    public record UserWorkoutBlockRDTO : BaseRDTO
    {
        public int BlockedUserId { get; set; }
        public string? BlockedUserName { get; set; }
        public DateTime BlockedUntil { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
