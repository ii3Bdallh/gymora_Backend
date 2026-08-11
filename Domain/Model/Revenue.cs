using Domain.Attributes;
using Domain.Enum;
using Domain.Model.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class Revenue : BaseAuditableGymEntity
    {
        [Filterable(FilterType.Exact)]
        public RevenueCategory RevenueCategory { get; set; }

        [Filterable(FilterType.Exact)]
        public int? GymMemberId { get; set; }
        public GymPerson? GymMember { get; set; }

        public decimal Amount { get; set; }

        [Filterable(FilterType.Exact)]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(100)]
        [Searchable]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime RevenueDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
