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

        public ICollection<SessionExercise> Exercises { get; set; } = new List<SessionExercise>();
        public ICollection<WorkoutPlanSession> PlanSessions { get; set; } = new List<WorkoutPlanSession>();
    }
}
