using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Domain.Model.Base;
using Infrastructure.Cache;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace Infrastructure.Repo.Entity
{
    public class SubscriptionPlanRepo(ApplicationDbContext context, ILogger<SubscriptionPlanRepo> logger, QueryCache queryCache)
    : BaseRepo<SubscriptionPlan>(context, logger, queryCache), ISubscriptionPlanRepo
    {

        protected override Func<IQueryable<SubscriptionPlan>, IQueryable<SubscriptionPlan>>? Includes()
        {
            return query => query
                .Include(x => x.Prices);
        }

        public override Task<PaginatedRes<SubscriptionPlan>> GetPageAsync(PaginatedSearchReq searchReq,
          bool trackChanges = false,
           CancellationToken cancellationToken = default,
            Func<IQueryable<SubscriptionPlan>, IQueryable<SubscriptionPlan>>? include = null)

        {
            include ??= Includes();
            return base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }

        public override async Task<SubscriptionPlan?> GetByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default, Func<IQueryable<SubscriptionPlan>, IQueryable<SubscriptionPlan>>? include = null)
        {
            include ??= Includes();
            return await base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }


        public Task<PlanPrice> AddPlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default)
        {
            // العملية تتم في الـ Memory فقط
            context.PlanPrice.Add(planPrice);
            return Task.FromResult(planPrice);
        }

        public Task<PlanPrice> DeletePlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default)
        {

            context.PlanPrice.Remove(planPrice);
            return Task.FromResult(planPrice);
        }




        public Task<PlanPrice?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return context.PlanPrice
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<PlanPrice?> GetPlanPriceByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            IQueryable<PlanPrice> query = trackChanges ? context.PlanPrice : context.PlanPrice.AsNoTracking();

            return await query
                .Include(x => x.Plan)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }



        public async Task<SubscriptionPlan?> GetFreePlanAsync(CancellationToken ct = default)
        {
            return await context.SubscriptionPlan
              .AsNoTracking()
              .FirstOrDefaultAsync(x => x.IsFree, ct);
        }
    }
}

