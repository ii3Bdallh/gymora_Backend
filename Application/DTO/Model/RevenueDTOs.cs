using Application.DTO.Base;
using Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Model
{
    public record RevenueCDTO : BaseGymAuditableCDTO
    {
        [Required(ErrorMessage = "RevenueCategory is required.")]
        public RevenueCategory RevenueCategory { get; init; }

        [Range(1, int.MaxValue)]
        public int? GymMemberId { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        public PaymentMethod PaymentMethod { get; init; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        [Required(ErrorMessage = "RevenueDate is required.")]
        public DateTime RevenueDate { get; init; }
    }

    public record RevenueUDTO : BaseGymAuditableUDTO
    {
        [Required(ErrorMessage = "RevenueCategory is required.")]
        public RevenueCategory RevenueCategory { get; init; }

        [Range(1, int.MaxValue)]
        public int? GymMemberId { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        public PaymentMethod PaymentMethod { get; init; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        [Required(ErrorMessage = "RevenueDate is required.")]
        public DateTime RevenueDate { get; init; }
    }

    public record RevenueRDTO : BaseGymAuditableRDTO
    {
        public RevenueCategory RevenueCategory { get; init; }
        public int? GymMemberId { get; init; }
        public string? GymMemberName { get; init; }
        public GymPersonRDTO? GymMember { get; init; }
        public GymPersonRDTO? CreatedByPerson { get; init; }
        public decimal Amount { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? Description { get; init; }
        public DateTime RevenueDate { get; init; }
    }
}
