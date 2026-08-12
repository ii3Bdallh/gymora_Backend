using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record    SessionExerciseCDTO : BaseCDTO
    {
        [Required(ErrorMessage = "Session ID is required")]
        public int SessionId { get; set; }

        public int? ExerciseId { get; set; }

        [Required(ErrorMessage = "Exercise name is required")]
        [MaxLength(200, ErrorMessage = "Exercise name must not exceed 200 characters")]
        public string ExerciseName { get; set; } = null!;

        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public int? RestSeconds { get; set; }

        [MaxLength(300, ErrorMessage = "Notes must not exceed 300 characters")]
        public string? Notes { get; set; }

        public int OrderIndex { get; set; } = 0;
    }

    public record SessionExerciseUDTO : BaseUDTO
    {
        [Required(ErrorMessage = "Session ID is required")]
        public int SessionId { get; set; }

        public int? ExerciseId { get; set; }

        [Required(ErrorMessage = "Exercise name is required")]
        [MaxLength(200, ErrorMessage = "Exercise name must not exceed 200 characters")]
        public string ExerciseName { get; set; } = null!;

        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public int? RestSeconds { get; set; }

        [MaxLength(300, ErrorMessage = "Notes must not exceed 300 characters")]
        public string? Notes { get; set; }

        public int OrderIndex { get; set; } = 0;
    }

    public record SessionExerciseRDTO : BaseRDTO
    {
        public int SessionId { get; set; }
        public int? ExerciseId { get; set; }
        public string ExerciseName { get; set; } = null!;
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public int? RestSeconds { get; set; }
        public string? Notes { get; set; }
        public int OrderIndex { get; set; }
    }
}
