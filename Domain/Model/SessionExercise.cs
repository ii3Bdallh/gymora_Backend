using System.ComponentModel.DataAnnotations;
using Domain.Model.Base;

namespace Domain.Model
{
    public class SessionExercise : BaseEntity
    {
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int? ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }

        [Required]
        [MaxLength(200)]
        public string ExerciseName { get; set; } = null!;

        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public int? RestSeconds { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public int OrderIndex { get; set; } = 0;
    }
}
