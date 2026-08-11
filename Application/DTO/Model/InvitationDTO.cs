using Application.DTO.Base;
using Application.DTO.Pagintion;
using Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Model
{
    // ── Nested DTOs for Invitation profiles ─────────────────────────

    /// <summary>
    /// Membership details provided when inviting a Member.
    /// These values will be used automatically to create GymMemberProfile on accept.
    /// </summary>
    public record InvitationMembershipDTO
    {
        /// <summary>Required: link to an existing MembershipPlan</summary>
        [Required(ErrorMessage = "MembershipPlanId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid MembershipPlanId.")]
        public int? MembershipPlanId { get; init; }

        public decimal DiscountAmount { get; init; }

        
    }

    /// <summary>
    /// Salary details provided when inviting a Staff member.
    /// These values will be used automatically to create GymStaffProfile on accept.
    /// </summary>
    public record InvitationSalaryDTO
    {
        [Range(0, double.MaxValue)]
        public decimal Salary { get; init; }

        public DateTime SalaryValidFrom { get; init; } = DateTime.UtcNow;

        public DateTime SalaryValidUntil { get; init; } = DateTime.UtcNow.AddMonths(1);
    }

    // ── Main Invitation DTOs ─────────────────────────────────────────

    public record InvitationCDTO : BaseGymAuditableCDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid UserId.")]
        public int UserId { get; init; }

        [Required(ErrorMessage = "GymRole is required.")]
        public GymRole GymRole { get; init; }

        /// <summary>
        /// Required when GymRole == Member. Defines the membership plan that will be
        /// created automatically once the invited person accepts.
        /// </summary>
        public InvitationMembershipDTO? Membership { get; init; }

        /// <summary>
        /// Required when GymRole != Member. Defines the salary details that will be
        /// created automatically once the invited person accepts.
        /// </summary>
        public InvitationSalaryDTO? Salary { get; init; }


    }

    public record InvitationUDTO : BaseGymAuditableUDTO
    {
        public InvitationStatus Status { get; init; }
    }

    public record InvitationRDTO : BaseGymAuditableRDTO
    {
        public int UserId { get; init; }
        public GymRole GymRole { get; init; }
        public InvitationStatus Status { get; init; }
        public DateTime? AcceptedAt { get; init; }
        public DateTime? RejectedAt { get; init; }

        // Membership snapshot (for Member invitations)
        public int? MembershipPlanId { get; init; }
        public string? PlanName { get; init; }
        public int? DurationDays { get; init; }
        public decimal? Amount { get; init; }
        public decimal? DiscountAmount { get; init; }
        public decimal? FinalAmount { get; init; }

        // Salary snapshot (for Staff invitations)
        public decimal? Salary { get; init; }
        public DateTime? SalaryValidFrom { get; init; }
        public DateTime? SalaryValidUntil { get; init; }
    }

    public class GetMyInvitationsPagedReq : PaginatedSearchReq
    {
    }
}
