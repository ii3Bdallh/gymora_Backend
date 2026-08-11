using Domain.Attributes;
using Domain.Enum;
using Domain.Model.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class Expense : BaseAuditableGymEntity
    {
        [Filterable(FilterType.Exact)]
        public ExpenseCategory ExpenseCategory { get; set; }

        [Filterable(FilterType.Exact)]
        public int? GymStaffId { get; set; }
        public GymPerson? GymStaff { get; set; }

        public decimal Amount { get; set; }

        [Filterable(FilterType.Exact)]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(100)]
        [Searchable]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? ReceiptUrl { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime ExpenseDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
