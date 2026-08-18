
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Domain.Enum;
using Domain.Model;

namespace Application.DTO.Model
{
    public record CouponCDTO : BaseCDTO
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [EnumDataType(typeof(DiscountType))]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }

        [Range(0.00, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0.00, double.MaxValue)]
        public decimal? MinimumPurchaseAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int UsageLimit { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        public bool IsFirstPurchaseOnly { get; set; }
    }

    public record CouponUDTO : BaseUDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.00, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0.00, double.MaxValue)]
        public decimal? MinimumPurchaseAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsageLimit { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

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
    }
}