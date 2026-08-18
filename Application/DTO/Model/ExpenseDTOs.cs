using Application.DTO.Base;
using Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Model
{
    public record ExpenseCDTO : BaseGymAuditableCDTO
    {
        [Required(ErrorMessage = "ExpenseCategory is required.")]
        public ExpenseCategory ExpenseCategory { get; init; }

        [Range(1, int.MaxValue)]
        public int? GymStaffId { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        public PaymentMethod PaymentMethod { get; init; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; init; }

        [MaxLength(500)]
        public string? ReceiptUrl { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        [Required(ErrorMessage = "ExpenseDate is required.")]
        public DateTime ExpenseDate { get; init; }
    }

    public record ExpenseUDTO : BaseGymAuditableUDTO
    {
        [Required(ErrorMessage = "ExpenseCategory is required.")]
        public ExpenseCategory ExpenseCategory { get; init; }

        [Range(1, int.MaxValue)]
        public int? GymStaffId { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; init; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        public PaymentMethod PaymentMethod { get; init; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; init; }

        [MaxLength(500)]
        public string? ReceiptUrl { get; init; }

        [MaxLength(500)]
        public string? Description { get; init; }

        [Required(ErrorMessage = "ExpenseDate is required.")]
        public DateTime ExpenseDate { get; init; }
    }

    public record ExpenseRDTO : BaseGymAuditableRDTO
    {
        public ExpenseCategory ExpenseCategory { get; init; }
        public int? GymStaffId { get; init; }
        public string? GymStaffName { get; init; }
        public GymPersonRDTO? GymStaff { get; init; }
        public GymPersonRDTO? CreatedByPerson { get; init; }
        public decimal Amount { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? ReceiptUrl { get; init; }
        public string? Description { get; init; }
        public DateTime ExpenseDate { get; init; }
    }
}
