using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class WorkoutPlanRepo(ApplicationDbContext context, ILogger<WorkoutPlanRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseRepo<WorkoutPlan>(context, logger, queryCache), IWorkoutPlanRepo
    {
        protected readonly CurrentUser _currentUser = currentUser;

        protected override Func<IQueryable<WorkoutPlan>, IQueryable<WorkoutPlan>>? Includes()
        {
            return query => query.Include(x => x.Sessions)
                                 .ThenInclude(s => s.Session)
                                 .ThenInclude(ws => ws.Exercises);
        }

        protected override IQueryable<WorkoutPlan> ApplyExtraFilters(IQueryable<WorkoutPlan> query)
        {
            query = base.ApplyExtraFilters(query);

            if (_currentUser.IsSuperAdmin)
                return query;

            // Non-SuperAdmins can only see approved workout plans or workout plans they created themselves
            return query.Where(x => x.IsApproved || x.CreatedById == _currentUser.UserId);
        }

        public override async Task<WorkoutPlan?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<WorkoutPlan>, IQueryable<WorkoutPlan>>? include = null)
        {
            include ??= Includes();
            return await base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }

        public override async Task<PaginatedRes<WorkoutPlan>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<WorkoutPlan>, IQueryable<WorkoutPlan>>? include = null)
        {
            // include ??= Includes();
            return await base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }
    }
}
