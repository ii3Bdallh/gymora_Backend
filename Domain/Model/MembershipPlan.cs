using Domain.Attributes;
using Domain.Model.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class MembershipPlan : BaseAuditableGymEntity
    {
        [Required]
        [MaxLength(100)]
        [Searchable]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        [Searchable]
        public string? Description { get; set; }

        [Filterable(FilterType.Exact)]
        public int DurationDays { get; set; }

        [Filterable(FilterType.Between)]
        public decimal Price { get; set; }

        [Filterable(FilterType.Exact)]
        public int FreezeDaysLimit { get; set; }

        public int? AttendanceLimit { get; set; }

        [Filterable(FilterType.Exact)]
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
