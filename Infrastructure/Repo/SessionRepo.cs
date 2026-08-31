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
    public class SessionRepo(ApplicationDbContext context, ILogger<SessionRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseRepo<Session>(context, logger, queryCache), ISessionRepo
    {
        private readonly CurrentUser _currentUser = currentUser;

        protected override Func<IQueryable<Session>, IQueryable<Session>>? Includes()
        {
            return query => query.Include(x => x.Exercises);
        }

        protected override IQueryable<Session> ApplyExtraFilters(IQueryable<Session> query)
        {
            query = base.ApplyExtraFilters(query);

            if (_currentUser.IsSuperAdmin)
                return query;

            // Non-SuperAdmins can only see approved sessions or sessions they created themselves
            return query.Where(x => x.IsApproved || x.CreatedById == _currentUser.UserId);
        }

        public override async Task<Session?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
            => await base.GetByIdAsync(id, false, cancellationToken, Includes());

        public override Task<Session?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Session>, IQueryable<Session>>? include = null)
        {
            return base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }

        public override Task<PaginatedRes<Session>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Session>, IQueryable<Session>>? include = null)
        {
            return base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }
    }
}
