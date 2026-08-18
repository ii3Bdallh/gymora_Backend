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
    public class OwnerSubscriptionRepo(ApplicationDbContext context, ILogger<OwnerSubscriptionRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseAuditableRepo<OwnerSubscription>(context, logger, queryCache, currentUser), IOwnerSubscriptionRepo
    {
        protected override Func<IQueryable<OwnerSubscription>, IQueryable<OwnerSubscription>>? Includes()
        {
            return query => query.Include(x => x.Plan).Include(x => x.PlanPrice).Include(x => x.PaymentRequest);
        }

        public override  Task<OwnerSubscription?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return  base.GetByIdAsync(id, false, cancellationToken, Includes());
        }

        public async Task<bool> HasActiveSubscriptionAsync(int ownerUserId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(x =>
                x.CreatedById == ownerUserId &&

               DateTime.UtcNow <= x.EndDate, ct);
        }

        public async Task<OwnerSubscription?> GetCurrentSubscriptionAsync(int ownerUserId, CancellationToken ct = default)
        {
            return await DbSet
                .Include(x => x.Plan)
                .Include(x => x.PlanPrice)
                .FirstOrDefaultAsync(x =>
                    x.CreatedById == ownerUserId &&
                    DateTime.UtcNow <= x.EndDate, ct);
        }
    }
}