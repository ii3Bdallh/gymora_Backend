
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record CouponRedemptionCDTO : BaseAuditableCDTO
    {
        public int CouponId { get; set; }
        public int PaymentRequestId { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public record CouponRedemptionUDTO : BaseAuditableUDTO
    {
        public decimal DiscountAmount { get; set; }
    }

    public record CouponRedemptionRDTO : BaseAuditableRDTO
    {
        public int CouponId { get; set; }
        public int PaymentRequestId { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}