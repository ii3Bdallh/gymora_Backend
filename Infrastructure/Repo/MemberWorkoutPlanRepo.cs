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
    public class MemberWorkoutPlanRepo(ApplicationDbContext context, ILogger<MemberWorkoutPlanRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<MemberWorkoutPlan>(context, logger, queryCache, currentUser), IMemberWorkoutPlanRepo
    {
        protected override Func<IQueryable<MemberWorkoutPlan>, IQueryable<MemberWorkoutPlan>>? Includes()
        {
            return query => query.Include(x => x.WorkoutPlan)
                                 .Include(x => x.Member);
        }

        public override async Task<MemberWorkoutPlan?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<MemberWorkoutPlan>, IQueryable<MemberWorkoutPlan>>? include = null)
        {
            include ??= Includes();
            return await base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }

        public override  Task<PaginatedRes<MemberWorkoutPlan>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<MemberWorkoutPlan>, IQueryable<MemberWorkoutPlan>>? include = null)
        {
            // include ??= Includes();
            return  base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }
    }
}
