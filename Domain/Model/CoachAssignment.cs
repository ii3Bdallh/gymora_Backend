using Domain.Model.Base;
using System;

namespace Domain.Model
{
    public class CoachAssignment : BaseGymEntity
    {
        public int MemberId { get; set; }
        public GymPerson Member { get; set; } = null!;

        public int CoachStaffId { get; set; }
        public GymPerson Coach { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public int AssignedById { get; set; }
        public GymPerson AssignedBy { get; set; } = null!;

        public DateTime? EndedAt { get; set; }
    }
}
