using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Model.Base;

namespace Domain.Model
{
    public class WorkoutPlan : BaseAuditableFileEntity
    {


        [Required]
        [MaxLength(200)]
        public string PlanName { get; set; } = null!;

        [Searchable]
        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public ICollection<WorkoutPlanSession> Sessions { get; set; } = new List<WorkoutPlanSession>();
    }
}
