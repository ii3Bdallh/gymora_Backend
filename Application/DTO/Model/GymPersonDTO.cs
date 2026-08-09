using System;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Domain.Enum;
using Domain.Model;

namespace Application.DTO.Model
{
    // ==========================================
    // Staff Profile DTOs
    // ==========================================
    public record GymStaffProfileCDTO
    {
        public GymRole GymRoleId { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? SalaryValidFrom { get; set; }
        public DateTime? SalaryValidUntil { get; set; }
    }

    public record GymStaffProfileUDTO
    {
        public GymRole GymRoleId { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? SalaryValidFrom { get; set; }
        public DateTime? SalaryValidUntil { get; set; }
    }

    public record GymStaffProfileRDTO
    {
        public GymRole GymRoleId { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? SalaryValidFrom { get; set; }
        public DateTime? SalaryValidUntil { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }

    // ==========================================
    // Member Profile DTOs
    // ==========================================
    public record GymMemberProfileCDTO
    {
        public string? MedicalNotes { get; set; }
        public string? Notes { get; set; }
    }

    public record GymMemberProfileUDTO
    {
        public string? MedicalNotes { get; set; }
        public string? Notes { get; set; }
    }

    public record GymMemberProfileRDTO
    {
        public string? MedicalNotes { get; set; }
        public string? Notes { get; set; }
    }

    // ==========================================
    // Gym Person DTOs
    // ==========================================
    public record GymPersonCDTO : BaseGymCDTO
    {
        public int? UserId { get; set; }
        public PersonType PersonType { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }

        public int CreatedById { get; set; }

        public GymStaffProfileCDTO? StaffProfile { get; set; }
        public GymMemberProfileCDTO? MemberProfile { get; set; }
    }

    public record GymPersonUDTO : BaseGymUDTO
    {
        public int? UserId { get; set; }
        public PersonType PersonType { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }
        public int CreatedById { get; set; }

        public GymStaffProfileUDTO? StaffProfile { get; set; }
        public GymMemberProfileUDTO? MemberProfile { get; set; }
    }

    public record GymPersonRDTO : BaseGymRDTO
    {
        public int? UserId { get; set; }
        public PersonType PersonType { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }
        public Guid InviteCode { get; set; }

        public GymStaffProfileRDTO? StaffProfile { get; set; }
        public GymMemberProfileRDTO? MemberProfile { get; set; }

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }

    }

    public record RenewMembershipDTO
    {
        [Required(ErrorMessage = "MembershipPlanId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid MembershipPlanId")]
        public int MembershipPlanId { get; init; }

        [Range(0.0, double.MaxValue, ErrorMessage = "PricePaid must be non-negative")]
        public decimal PricePaid { get; init; }

        [Range(0.0, double.MaxValue, ErrorMessage = "DiscountAmount must be non-negative")]
        public decimal DiscountAmount { get; init; }

        [Range(0.0, double.MaxValue, ErrorMessage = "FinalAmount must be non-negative")]
        public decimal FinalAmount { get; init; }


        [MaxLength(500)]
        public string? Notes { get; init; }
    }

    public record UpdateAccessStatusDTO
    {
        [Required(ErrorMessage = "Status is required")]
        public GymPersonAccessStatus Status { get; init; }
    }
}
