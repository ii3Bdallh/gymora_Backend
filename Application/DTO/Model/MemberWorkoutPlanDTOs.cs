using System;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Domain.Enum;

namespace Application.DTO.Model
{
    public record MemberWorkoutPlanCDTO : BaseGymAuditableCDTO
    {
        [Required(ErrorMessage = "Workout plan ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid WorkoutPlanId")]
        public int WorkoutPlanId { get; set; }

        [Required(ErrorMessage = "Member ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
        public int MemberId { get; set; }

        [MaxLength(200, ErrorMessage = "Goal must not exceed 200 characters")]
        public string? Goal { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public record MemberWorkoutPlanUDTO : BaseGymAuditableUDTO
    {
        [Required(ErrorMessage = "Workout plan ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid WorkoutPlanId")]
        public int WorkoutPlanId { get; set; }

        [Required(ErrorMessage = "Member ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
        public int MemberId { get; set; }

        [MaxLength(200, ErrorMessage = "Goal must not exceed 200 characters")]
        public string? Goal { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public MemberWorkoutPlanStatus Status { get; set; } = MemberWorkoutPlanStatus.Active;
    }

    public record MemberWorkoutPlanRDTO : BaseGymAuditableRDTO
    {
        public int WorkoutPlanId { get; set; }
        public string? WorkoutPlanName { get; set; }
        public int MemberId { get; set; }
        public string? MemberName { get; set; }
        public string? Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public MemberWorkoutPlanStatus Status { get; set; }
    }
}
