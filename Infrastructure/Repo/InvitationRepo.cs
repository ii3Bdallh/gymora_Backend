using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Model;
using Domain.Enum;
using Application.Interface.Repo;
using Application.DTO.Pagintion;
using Application.DTO.Model;
using Application.Model;
using Infrastructure.Cache;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class InvitationRepo(ApplicationDbContext context, ILogger<InvitationRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<Invitation>(context, logger, queryCache, currentUser), IInvitationRepo
    {
        public override IQueryable<Invitation> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Invitation>, IQueryable<Invitation>>? include = null)
        {
            if (searchReq is GetMyInvitationsPagedReq)
            {
                IQueryable<Invitation> query = trackChanges ? DbSet : DbSet.AsNoTracking();

                query = query.Where(x => x.UserId == currentUser.UserId);

                if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                    query = query.Search(searchReq.SearchTerm, queryCache);

                if (searchReq.Filters is not null)
                    query = query.ApplyFilters(searchReq.Filters, queryCache);

                var orderBy = !string.IsNullOrEmpty(searchReq.OrderBy) ? searchReq.OrderBy : "Id";
                var direction = searchReq.OrderDirection?.ToLower() == "desc" ? "descending" : "ascending";
                query = query.OrderBy($"{orderBy} {direction}");

                if (include != null) query = include(query);

                return query;
            }

            return base.GetAllQuery(searchReq, trackChanges, cancellationToken, include);
        }

        protected override Func<IQueryable<Invitation>, IQueryable<Invitation>>? Includes()
        {
            return query => query.Include(x => x.User);
        }

        public override async Task<Invitation?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await base.GetByIdAsync(id, false, cancellationToken, Includes());
        }

        public async Task<bool> HasPendingInvitationAsync(int gymId, int userId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(x =>
                x.GymId == gymId &&
                x.UserId == userId &&
                x.Status == InvitationStatus.Pending,
                ct);
        }
    }
}
