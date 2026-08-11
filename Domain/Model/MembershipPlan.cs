using Domain.Attributes;
using Domain.Interface;
using Domain.Model.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class MembershipPlan : BaseAuditableGymEntity, ICacheableEntity
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



        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
