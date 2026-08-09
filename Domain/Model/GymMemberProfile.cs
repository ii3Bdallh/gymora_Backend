
using Domain.Attributes;
using Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class GymMemberProfile
    {
        public int Id { get; set; }
        
        [MaxLength(1000)]
        public string? MedicalNotes { get; set; }
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        public GymPerson GymPerson { get; set; } = default!;


        [Filterable(FilterType.Exact)]
        public int? MembershipPlanId { get; set; }
        public MembershipPlan? MembershipPlan { get; set; }

        [Required]
        [MaxLength(100)]
        [Searchable]
        public string PlanName { get; set; } = "Basic";

        [Filterable(FilterType.Exact)]
        public int DurationDays { get; set; }

        [Filterable(FilterType.Between)]
        public decimal PricePaid { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal FinalAmount { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime? MembershipStartDate { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime? MembershipEndDate { get; set; }

    }
}
