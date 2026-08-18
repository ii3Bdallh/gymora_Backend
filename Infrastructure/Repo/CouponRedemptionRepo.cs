using Application.Interface.Repo;
using Domain.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Repo.Base;
using Domain.Model.Auth;
using Google;
using Infrastructure.Persistence;
using Infrastructure.Cache;
using Application.Model;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repo
{
    public class CouponRedemptionRepo(ApplicationDbContext context, ILogger<CouponRedemptionRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseAuditableRepo<CouponRedemption>(context, logger, queryCache, currentUser), ICouponRedemptionRepo
    {
        protected override Func<IQueryable<CouponRedemption>, IQueryable<CouponRedemption>>? Includes()
        {
            return query => query.Include(x => x.Coupon).Include(x => x.PaymentRequest);
        }

        public override  Task<CouponRedemption?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return  base.GetByIdAsync(id, false, cancellationToken, Includes());
        }
    }
}