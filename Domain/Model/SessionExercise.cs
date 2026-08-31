using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Model.Base;

namespace Domain.Model
{
    public class SessionExercise : BaseEntity
    {
        [Filterable(FilterType.Exact)]
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public int? ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }

        [Searchable]
        [Required]
        [MaxLength(200)]
        public string ExerciseName { get; set; } = null!;

        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public int? RestSeconds { get; set; }

        [Searchable]
        [MaxLength(300)]
        public string? Notes { get; set; }

        [Filterable(FilterType.Exact)]
        public int OrderIndex { get; set; } = 0;
    }
}
