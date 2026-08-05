using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Service.Base;


using Application.DTO.Model;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;
using Domain.Enum;

namespace Application.Service
{
    public class CouponService : BaseService<Coupon, CouponRDTO, CouponCDTO, CouponUDTO>, ICouponService
    {
        private readonly ICouponRepo _couponRepo;
        public CouponService(
            ICouponRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<CouponService> logger
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _couponRepo = repo;
        }

        public async Task IncrementUsageAsync(int couponId, CancellationToken ct = default)
        {

            Coupon? coupon = await _couponRepo.GetByIdAsync(couponId, true, ct);
            if (coupon == null) return;

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return;

            coupon.UsedCount++;
            await _unitOfWork.SaveChangesAsync(ct);

        }

        // نقص الاستخدام (عند Reject / Cancel)
        public async Task DecrementUsageAsync(int couponId, CancellationToken ct = default)
        {
            Coupon? coupon = await _couponRepo.GetByIdAsync(couponId, true, ct);
            if (coupon == null || coupon.UsedCount <= 0)
                return;

            coupon.UsedCount--;
            await _unitOfWork.SaveChangesAsync(ct);

        }

        public async Task<CouponValidationResult> ValidateCouponAsync(
                string code,
                decimal orderAmount,
                int planId,
                CancellationToken ct = default)
        {
            var coupon = await _couponRepo.GetByCodeAsync(code, ct);
            if (coupon == null)
                return CouponValidationResult.Failure("Coupon not found.");

            // Validation Logic (يمكن توسيعه)
            if (DateTime.UtcNow < coupon.ValidFrom || DateTime.UtcNow > coupon.ValidTo)
                return CouponValidationResult.Failure("Coupon is outside its valid period.");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
                return CouponValidationResult.Failure("Coupon has reached its usage limit.");

            if (coupon.MinimumPurchaseAmount.HasValue && orderAmount < coupon.MinimumPurchaseAmount)
                return CouponValidationResult.Failure("Order amount is below the minimum required for this coupon.");

            decimal discount = coupon.DiscountType == DiscountType.Percentage
                ? (orderAmount * coupon.DiscountValue / 100)
                : coupon.DiscountValue;

            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount)
                discount = coupon.MaxDiscountAmount.Value;

            return CouponValidationResult.Success(coupon.Id, discount, coupon);
        }


    }


}