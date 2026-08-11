using Application.Interface.Repo;
using Domain.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Repo.Base;
using Infrastructure.Persistence;
using Infrastructure.Cache;
using Application.Model;
using Microsoft.EntityFrameworkCore;
using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Enum;

namespace Infrastructure.Repo
{
    public class GymPersonRepo(ApplicationDbContext context, ILogger<GymPersonRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseGymRepo<GymPerson>(context, logger, queryCache, currentUser), IGymPersonRepo
    {
        protected override Func<IQueryable<GymPerson>, IQueryable<GymPerson>>? Includes()
        {
            return query => query
                .Include(x => x.StaffProfile)
                .Include(x => x.MemberProfile);
        }

        public override Task<PaginatedRes<GymPerson>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<GymPerson>, IQueryable<GymPerson>>? include = null)
        {
            include ??= Includes();
            return base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }

        public override Task<GymPerson?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<GymPerson>, IQueryable<GymPerson>>? include = null)
        {
            include ??= Includes();
            return base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }





        public async Task<GymPerson?> LinkAccountToGymAsync(
            int gymId,
            Guid inviteCode,
            CancellationToken ct = default)
        {
            var gymPerson = await context.GymPerson
                .Include(x => x.StaffProfile)
                .Include(x => x.MemberProfile)
                .FirstOrDefaultAsync(x => x.GymId == gymId && x.InviteCode == inviteCode, ct);

            if (gymPerson is null)
                throw new InvalidOperationException("Invalid invite code.");

            if (gymPerson.UserId != null)
                throw new InvalidOperationException("This person is already linked.");

            gymPerson.UserId = currentUser.UserId;

            return gymPerson;
        }

        public async Task<int> CountPeopleTypeByOwnerAsync(
            int ownerUserId,
            PersonType personType,
            CancellationToken ct = default)
        {
            var ownedGymIds = await context.GymPerson
                .Where(x => x.UserId == ownerUserId && x.PersonType == PersonType.Owner)
                .Select(x => x.GymId)
                .ToListAsync(ct);

            if (!ownedGymIds.Any())
                return 0;

            return await context.GymPerson.CountAsync(x =>
                ownedGymIds.Contains(x.GymId) &&
                (x.PersonType == personType || x.PersonType == PersonType.StaffMember),
                ct);
        }



        public async Task<GymPerson?> GetGymOwnerAsync(int gymId, CancellationToken ct = default)
        {
            return await DbSet.Where(x => x.GymId == gymId && x.PersonType == PersonType.Owner).FirstOrDefaultAsync(ct);
        }

        public async Task<GymPerson?> GetGymPersonAsync(int gymId, int userId, CancellationToken ct = default)
        {
            return await DbSet.Where(x => x.GymId == gymId && x.UserId == userId).FirstOrDefaultAsync(ct);
        }

        public async Task<GymPerson?> GetGymPersonByEmailAsync(int gymId, string email, CancellationToken ct = default)
        {
            return await DbSet.Where(x => x.GymId == gymId && x.Email == email).FirstOrDefaultAsync(ct);
        }

        public async Task<int> GetActiveMembersCountAsync(int gymId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await DbSet
                .Where(x => x.GymId == gymId && 
                            x.PersonType == PersonType.Member && 
                            x.MemberProfile != null && 
                            x.MemberProfile.MembershipEndDate.HasValue && 
                            x.MemberProfile.MembershipEndDate.Value > now && 
                            x.AccessStatus == GymPersonAccessStatus.Active)
                .CountAsync(ct);
        }

        public async Task<int> GetExpiredMembersCountAsync(int gymId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await DbSet
                .Where(x => x.GymId == gymId && 
                            x.PersonType == PersonType.Member && 
                            x.MemberProfile != null && 
                            (!x.MemberProfile.MembershipEndDate.HasValue || x.MemberProfile.MembershipEndDate.Value <= now) && 
                            x.AccessStatus == GymPersonAccessStatus.Active)
                .CountAsync(ct);
        }

        // public async Task<List<GymPerson>> GetMembersForReportAsync(int gymId, CancellationToken ct = default)
        // {
        //     return await DbSet
        //         .Include(x => x.MemberProfile)
        //         .Where(x => x.GymId == gymId && x.PersonType == PersonType.Member && x.MemberProfile != null)
        //         .ToListAsync(ct);
        // }
    }
}
