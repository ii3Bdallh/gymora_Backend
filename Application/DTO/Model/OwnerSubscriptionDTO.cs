
using Application.DTO.Base;
using Domain.Enum;
using Domain.Model;

namespace Application.DTO.Model
{
    public record OwnerSubscriptionCDTO : BaseAuditableCDTO
    {
        public int PlanId { get; set; }
        public int PlanPriceId { get; set; }
        public int? PaymentRequestId { get; set; }
        public decimal AmountPaid { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GraceEndDate { get; set; }
    }

    public record OwnerSubscriptionUDTO : BaseAuditableUDTO
    {
        public DateTime EndDate { get; set; }
        public DateTime GraceEndDate { get; set; }
    }

    public record OwnerSubscriptionRDTO : BaseAuditableRDTO
    {
        public int PlanId { get; set; }
        public int PlanPriceId { get; set; }
        public int? PaymentRequestId { get; set; }
        public decimal AmountPaid { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GraceEndDate { get; set; }
        public OwnerSubscriptionStatus Status
        {
            get; set;
        }
    }


    public class CurrentPlanResult
    {
        public int PlanId { get; init; }

        public string PlanName { get; init; } = null!;

        public int MaxOwnedGyms { get; init; }

        public int MaxMembers { get; init; }

        public int MaxCoaches { get; init; }

        public bool IsFree { get; init; }

        public OwnerSubscriptionStatus? SubscriptionStatus { get; init; }

        public OwnerSubscription? Subscription { get; init; }
    }
}