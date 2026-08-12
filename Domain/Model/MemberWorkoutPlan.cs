using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Enum;
using Domain.Model.Base;

namespace Domain.Model
{
    public class MemberWorkoutPlan : BaseAuditableGymEntity
    {
        [Filterable(FilterType.Exact)]
        public int WorkoutPlanId { get; set; }
        public WorkoutPlan WorkoutPlan { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public int MemberId { get; set; }
        public GymPerson Member { get; set; } = null!;
        


        [Searchable]
        [MaxLength(200)]
        public string? Goal { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime StartDate { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime? EndDate { get; set; }

        [Filterable(FilterType.Exact)]
        public MemberWorkoutPlanStatus Status { get; set; } = MemberWorkoutPlanStatus.Active;
    }
}
