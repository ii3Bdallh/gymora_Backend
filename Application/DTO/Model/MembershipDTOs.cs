using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Domain.Enum;

namespace Application.DTO.Model
{
    // --- MembershipPlan DTOs ---

    public record MembershipPlanCDTO : BaseGymAuditableCDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; init; } = null!;

        [MaxLength(500)]
        public string? Description { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = "DurationDays must be greater than zero")]
        public int DurationDays { get; init; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "FreezeDaysLimit must be non-negative")]
        public int FreezeDaysLimit { get; init; }

        public int? AttendanceLimit { get; init; }
    }

    public record MembershipPlanUDTO : BaseGymAuditableUDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; init; } = null!;

        [MaxLength(500)]
        public string? Description { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = "DurationDays must be greater than zero")]
        public int DurationDays { get; init; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "FreezeDaysLimit must be non-negative")]
        public int FreezeDaysLimit { get; init; }

        public int? AttendanceLimit { get; init; }
    }

    public record MembershipPlanRDTO : BaseGymAuditableRDTO
    {
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int DurationDays { get; init; }
        public decimal Price { get; init; }
        public int FreezeDaysLimit { get; init; }
        public int? AttendanceLimit { get; init; }
    }

    // --- Membership DTOs ---

  


}
