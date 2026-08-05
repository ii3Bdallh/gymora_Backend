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
            return query => query.Include(x => x.Coach).Include(x => x.Member).Include(x => x.AssignedBy);
        }

        public override IQueryable<CoachAssignment> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            var query = base.GetAllQuery(searchReq, isActive, trackChanges, cancellationToken, include);

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

        public override async Task<PaginatedRes<CoachAssignment>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            include ??= Includes();
            return await base.GetPageAsync(searchReq, isActive, trackChanges, cancellationToken, include);
        }

        public override async Task<CoachAssignment?> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<CoachAssignment>, IQueryable<CoachAssignment>>? include = null)
        {
            include ??= Includes();
            return await base.GetByIdAsync(id, isActive, trackChanges, cancellationToken, include);
        }

    }
}


