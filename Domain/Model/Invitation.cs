using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class Invitation : BaseAuditableGymEntity
    {
        [Required]
        [Filterable(FilterType.Exact)]
        public int UserId { get; set; }

        [Filterable(FilterType.Exact)]
        public GymRole GymRole { get; set; }

        [Filterable(FilterType.Exact)]
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime? AcceptedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        // ── Membership snapshot (stored when inviting a Member) ──────
        // Used to create GymMemberProfile automatically on accept

        public int? MembershipPlanId { get; set; }

        [MaxLength(100)]
        public string? PlanName { get; set; }

        public int? DurationDays { get; set; }

        public decimal? Amount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? FinalAmount { get; set; }

        // ── Salary snapshot (stored when inviting Staff) ─────────────
        // Used to create GymStaffProfile automatically on accept

        public decimal? Salary { get; set; }

        public DateTime? SalaryValidFrom { get; set; }

        public DateTime? SalaryValidUntil { get; set; }



        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
