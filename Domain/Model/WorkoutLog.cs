using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Model.Base;

namespace Domain.Model
{
    public class WorkoutLog : BaseGymEntity
    {
        [Filterable(FilterType.Exact)]
        public int MemberId { get; set; }
        public GymPerson Member { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public int SessionExerciseId { get; set; }
        public SessionExercise SessionExercise { get; set; } = null!;

        [Filterable(FilterType.Between)]
        public DateTime PerformedDate { get; set; }

        public int SetsCompleted { get; set; }
        public int RepsCompleted { get; set; }
        public decimal WeightUsed { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
