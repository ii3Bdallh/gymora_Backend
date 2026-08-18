using Application.DTO.Model;
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
    public class CoachAssignmentRepo(ApplicationDbContext context, ILogger<CoachAssignmentRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<CoachAssignment>(context, logger, queryCache, currentUser), ICoachAssignmentRepo
    {

        protected override Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? Includes()
        {
            return query => query.Include(x => x.Coach).Include(x => x.Member);
        }

        public override IQueryable<CoachAssignment> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            var query = base.GetAllQuery(searchReq, trackChanges, cancellationToken, include);

            if (searchReq is GetAssignedMemberForCoachPagedReq getAssignedMemberForCoachPagedReq)
            {
                query = query.Where(x => x.CoachStaffId == getAssignedMemberForCoachPagedReq.CoachId);
            }
            else if (searchReq is GetAssignCoachForMemberPagedReq getAssignCoachForMemberPagedReq)
            {
                query = query.Where(x => x.MemberId == getAssignCoachForMemberPagedReq.MemberId);
            }

            return query;
        }

        public override Task<PaginatedRes<CoachAssignment>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            include ??= Includes();
            return base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }

        public override Task<CoachAssignment?> GetByIdDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return base.GetByIdAsync(id, false, cancellationToken, query => query.Include(x => x.Coach).Include(x => x.Member).Include(x => x.AssignedBy));
        }

    }
}


