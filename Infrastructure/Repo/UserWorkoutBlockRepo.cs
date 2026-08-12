using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class UserWorkoutBlockRepo(ApplicationDbContext context, ILogger<UserWorkoutBlockRepo> logger, QueryCache queryCache)
        : BaseRepo<UserWorkoutBlock>(context, logger, queryCache), IUserWorkoutBlockRepo
    {
        protected override Func<IQueryable<UserWorkoutBlock>, IQueryable<UserWorkoutBlock>>? Includes()
        {
            return query => query.Include(x => x.BlockedUser);
        }

        public override async Task<PaginatedRes<UserWorkoutBlock>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<UserWorkoutBlock>, IQueryable<UserWorkoutBlock>>? include = null)
        {
            include ??= Includes();
            return await base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }

        public override async Task<UserWorkoutBlock?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<UserWorkoutBlock>, IQueryable<UserWorkoutBlock>>? include = null)
        {
            include ??= Includes();
            return await base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }
    }
}
