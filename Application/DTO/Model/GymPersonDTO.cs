using System;
using Application.DTO.Base;
using Domain.Enum;

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
    public record GymPersonCDTO : BaseAuditableCDTO
    {
        public int? UserId { get; set; }
        public PersonType PersonType { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }

        public GymStaffProfileCDTO? StaffProfile { get; set; }
        public GymMemberProfileCDTO? MemberProfile { get; set; }
    }

    public record GymPersonUDTO : BaseAuditableUDTO
    {
        public int? UserId { get; set; }
        public PersonType PersonType { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhotoUrl { get; set; }

        public GymStaffProfileUDTO? StaffProfile { get; set; }
        public GymMemberProfileUDTO? MemberProfile { get; set; }
    }

    public record GymPersonRDTO : BaseAuditableRDTO
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
    }
}
