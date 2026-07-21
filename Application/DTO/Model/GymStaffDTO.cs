
using Application.DTO.Base;
using Domain.Enum;

namespace Application.DTO.Model
{
    public record GymStaffCDTO : BaseCDTO
    {

        public string StaffName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public GymRole GymRole { get; set; }

        public decimal? Salary { get; set; }

    }

    public record GymStaffUDTO : BaseUDTO
    {
        public string StaffName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public GymRole GymRole { get; set; }

        public decimal? Salary { get; set; }
    }

    public record GymStaffRDTO : BaseRDTO
    {
        public int UserId { get; set; } // FK -> ApplicationUser, nullable: unregistered staff
        public string StaffName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public GymRole GymRole { get; set; }

        public decimal? Salary { get; set; }
        public Guid StaffInviteCode { get; set; }
        public DateTime? SalaryEffectiveFrom { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;


        ApplicationUserRDTO? User { get; set; } // Navigation property to ApplicationUser
    }
}