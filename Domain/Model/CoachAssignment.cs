using Domain.Attributes;
using Domain.Model.Base;
using System;

namespace Domain.Model
{
    public class CoachAssignment : BaseGymEntity
    {
        [Filterable(FilterType.Exact)]
        public int MemberId { get; set; }
        public GymPerson Member { get; set; } = null!;
        [Filterable(FilterType.Exact)]
        public int CoachStaffId { get; set; }
        public GymPerson Coach { get; set; } = null!;
        [Filterable(FilterType.Between)]

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        [Filterable(FilterType.Exact)]

        public int AssignedById { get; set; }
        public GymPerson AssignedBy { get; set; } = null!;

        [Filterable(FilterType.Between)]
        public DateTime? EndedAt { get; set; }
    }
}
