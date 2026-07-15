
using Application.DTO.Base;
using Domain.Enum;

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
}