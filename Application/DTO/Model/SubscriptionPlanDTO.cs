using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Base;
using Domain.Model;

namespace Application.DTO.Model
{

    public record SubscriptionPlanCDTO : BaseCDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;
        public bool IsFree { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxOwnedGyms must be non-negative.")]
        public int MaxOwnedGyms { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxCoaches must be non-negative.")]
        public int MaxCoaches { get; set; } // MaxCoaches

        [Range(0, int.MaxValue, ErrorMessage = "MaxMembers must be non-negative.")]
        public int MaxMembers { get; set; } // MaxMembers
        public string? FeaturesJson { get; set; }

        public ICollection<PlanPriceCDTO> Prices { get; set; } = new List<PlanPriceCDTO>();
    }

    public record SubscriptionPlanRDTO : BaseRDTO
    {
        public string Name { get; set; } = null!;
        public bool IsFree { get; set; }

        public string? Description { get; set; }
        public int MaxOwnedGyms { get; set; }
        public int MaxCoaches { get; set; }
        public int MaxMembers { get; set; }
        public string? FeaturesJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }

        public ICollection<PlanPriceRDTO> Prices { get; set; } = new List<PlanPriceRDTO>();
    }

    public record SubscriptionPlanUDTO : BaseUDTO
    {
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        public bool IsFree { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxOwnedGyms must be non-negative.")]
        public int MaxOwnedGyms { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxCoaches must be non-negative.")]
        public int MaxCoaches { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxMembers must be non-negative.")]
        public int MaxMembers { get; set; }
        public string? FeaturesJson { get; set; }
        public bool IsActive { get; set; }

        // public ICollection<PlanPriceUDTO> Prices { get; set; } = new List<PlanPriceUDTO>();
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; private set; }
        public string? Message { get; private set; }
        public int? CouponId { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public Coupon? Coupon { get; private set; }

        private CouponValidationResult(bool isValid, string? message, int? couponId, decimal discount)
        {
            IsValid = isValid;
            Message = message;
            CouponId = couponId;
            DiscountAmount = discount;
        }

        public static CouponValidationResult Success(int couponId, decimal discount, Coupon? coupon = null)
            => new CouponValidationResult(true, null, couponId, discount) { Coupon = coupon };

        public static CouponValidationResult Failure(string message)
            => new CouponValidationResult(false, message, null, 0);
    }

}