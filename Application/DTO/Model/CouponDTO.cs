
using Application.DTO.Base;
using Domain.Enum;
using Domain.Model;

namespace Application.DTO.Model
{
    public record CouponCDTO : BaseCDTO
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }
        public int UsageLimit { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsFirstPurchaseOnly { get; set; }
    }

    public record CouponUDTO : BaseUDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    public record CouponRDTO : BaseRDTO
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsFirstPurchaseOnly { get; set; }
        public bool IsActive { get; set; }
    }
}