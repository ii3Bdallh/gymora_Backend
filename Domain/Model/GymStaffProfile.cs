using Domain.Enum;
using System;

namespace Domain.Model
{
    public class GymStaffProfile
    {
        public int Id { get; set; }
        public GymRole GymRoleId { get; set; }

        public decimal? Salary { get; set; }

        public DateTime? SalaryValidFrom { get; set; }

        public DateTime? SalaryValidUntil { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        // Navigation property (1-to-1 with GymPerson)
        public GymPerson GymPerson { get; set; } = default!;
    }
}
