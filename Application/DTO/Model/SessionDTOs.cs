using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record SessionCDTO : BaseAuditableCDTO
    {
        [Required(ErrorMessage = "Workout Plan ID is required")]
        public int WorkoutPlanId { get; set; }

        [Required(ErrorMessage = "Day number is required")]
        [Range(1, 365, ErrorMessage = "Day number must be between 1 and 365")]
        public int DayNumber { get; set; }

        [Required(ErrorMessage = "Session name is required")]
        [MaxLength(100, ErrorMessage = "Session name must not exceed 100 characters")]
        public string SessionName { get; set; } = null!;

        public ICollection<SessionExerciseCDTO> Exercises { get; set; } = new List<SessionExerciseCDTO>();
    }

    public record SessionUDTO : BaseAuditableUDTO
    {
        [Required(ErrorMessage = "Workout Plan ID is required")]
        public int WorkoutPlanId { get; set; }

        [Required(ErrorMessage = "Day number is required")]
        [Range(1, 365, ErrorMessage = "Day number must be between 1 and 365")]
        public int DayNumber { get; set; }

        [Required(ErrorMessage = "Session name is required")]
        [MaxLength(100, ErrorMessage = "Session name must not exceed 100 characters")]
        public string SessionName { get; set; } = null!;
    }

    public record SessionRDTO : BaseAuditableRDTO
    {
        public int WorkoutPlanId { get; set; }
        public int DayNumber { get; set; }
        public string SessionName { get; set; } = null!;
        public bool IsApproved { get; set; }
        public ICollection<SessionExerciseRDTO> Exercises { get; set; } = new List<SessionExerciseRDTO>();
    }
}
