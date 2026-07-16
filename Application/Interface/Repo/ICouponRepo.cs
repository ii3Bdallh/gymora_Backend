using Application.DTO.Model;
using Application.DTO.Pagintion;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface ICouponRepo : IBaseRepo<Coupon>
    {
        Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);

        Task<Coupon?> IncrementUsageAsync(Coupon entity, CancellationToken ct = default);



    }
}