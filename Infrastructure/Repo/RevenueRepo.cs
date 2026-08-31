using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class RevenueRepo(ApplicationDbContext context, ILogger<RevenueRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<Revenue>(context, logger, queryCache, currentUser), IRevenueRepo
    {
        protected override Func<IQueryable<Revenue>, IQueryable<Revenue>>? Includes()
        {
            return query => query.Include(x => x.GymMember).Include(x => x.CreatedByPerson);
        }

        public override Task<PaginatedRes<Revenue>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Revenue>, IQueryable<Revenue>>? include = null)
        {
            // include ??= Includes();
            return base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }

        public override async Task<Revenue?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
            => await base.GetByIdAsync(id, false, cancellationToken, Includes());
    }
}
