using Application.DTO.Model;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface ICouponService : IBaseService<Coupon, CouponRDTO, CouponCDTO, CouponUDTO>
    {
        Task<CouponValidationResult> ValidateCouponAsync(string code, decimal orderAmount, int planId, CancellationToken ct = default);


        Task IncrementUsageAsync(int couponId, CancellationToken ct = default);
        Task DecrementUsageAsync(int couponId, CancellationToken ct = default);
    }
}