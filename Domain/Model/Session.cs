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

        public bool IsApproved { get; set; } = false;

        public int WorkoutPlanId { get; set; }
        public WorkoutPlan WorkoutPlan { get; set; } = null!;

        public int DayNumber { get; set; }

        public ICollection<SessionExercise> Exercises { get; set; } = new List<SessionExercise>();
    }
}
