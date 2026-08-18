
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record CouponRedemptionCDTO : BaseAuditableCDTO
    {

    }

    public record CouponRedemptionUDTO : BaseAuditableUDTO
    {

    }

    public record CouponRedemptionRDTO : BaseAuditableRDTO
    {
        public int CouponId { get; set; }
        public int PaymentRequestId { get; set; }
        public decimal DiscountAmount { get; set; }

        public CouponRDTO? Coupon { get; set; }

        public PaymentRequestRDTO? PaymentRequest { get; set; }
    }
}