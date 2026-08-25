using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Model.Auth;
using Domain.Model.Base;

namespace Domain.Model
{
    public class UserWorkoutBlock : BaseEntity
    {
        [Filterable(FilterType.Exact)]
        public int BlockedUserId { get; set; }
        public ApplicationUser BlockedUser { get; set; } = null!;

        [Filterable(FilterType.Between)]
        public DateTime BlockedUntil { get; set; }

        [Searchable]
        [MaxLength(500)]
        public string? Reason { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
