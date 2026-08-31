using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Model.Base;

namespace Domain.Model
{
    public class Session : BaseAuditableEntity
    {
        [Searchable]
        [Required]
        [MaxLength(100)]
        public string SessionName { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public bool IsApproved { get; set; } = false;

        [Filterable(FilterType.Exact)]
        public int WorkoutPlanId { get; set; }
        public WorkoutPlan WorkoutPlan { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public int DayNumber { get; set; }

        public ICollection<SessionExercise> Exercises { get; set; } = new List<SessionExercise>();
    }
}
