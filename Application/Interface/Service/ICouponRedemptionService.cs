using Application.DTO.Model;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface ICouponRedemptionService : IBaseService<CouponRedemption, CouponRedemptionRDTO, CouponRedemptionCDTO, CouponRedemptionUDTO>
    {
    }
}