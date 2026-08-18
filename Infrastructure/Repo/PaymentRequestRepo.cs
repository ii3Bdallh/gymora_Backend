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
using Domain.Enum;

namespace Infrastructure.Repo
{
    public class PaymentRequestRepo(ApplicationDbContext context, ILogger<PaymentRequestRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseAuditableRepo<PaymentRequest>(context, logger, queryCache, currentUser), IPaymentRequestRepo
    {
        protected override Func<IQueryable<PaymentRequest>, IQueryable<PaymentRequest>>? Includes()
        {
            return query => query.Include(x => x.Plan).Include(x => x.PlanPrice).Include(x => x.Coupon);
        }

        public override async Task<PaymentRequest?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, false, cancellationToken, Includes());
        }

        public async Task<bool> HasPendingRequestAsync(int UserId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(x =>
                x.CreatedById == UserId &&
                x.Status == PaymentRequestStatus.Pending, ct);
        }

        public async Task<bool> HasUsedThisCouponBeforeAsync(int UserId, int CouponId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(x =>
                x.CreatedById == UserId &&
                x.CouponId == CouponId &&
                x.Status == PaymentRequestStatus.Approved, ct);
        }
    }
}