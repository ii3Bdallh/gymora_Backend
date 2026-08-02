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
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<GymPerson>, IQueryable<GymPerson>>? include = null)
        {
            include ??= Includes();
            return base.GetPageAsync(searchReq, isActive, trackChanges, cancellationToken, include);
        }

        public override Task<GymPerson?> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<GymPerson>, IQueryable<GymPerson>>? include = null)
        {
            include ??= Includes();
            return base.GetByIdAsync(id, isActive, trackChanges, cancellationToken, include);
        }





        public async Task<GymPerson?> LinkAccountToGymAsync(
            int gymId,
            Guid inviteCode,
            CancellationToken ct = default)
        {
            var gymPerson = await context.GymPerson
                .Include(x => x.StaffProfile)
                .Include(x => x.MemberProfile)
                .FirstOrDefaultAsync(x => x.GymId == gymId && x.InviteCode == inviteCode && x.IsActive, ct);

            if (gymPerson is null)
                throw new InvalidOperationException("Invalid invite code.");

            if (gymPerson.UserId != null)
                throw new InvalidOperationException("This person is already linked.");

            gymPerson.UserId = currentUser.UserId;

            return gymPerson;
        }

        public Task<int> CountPeopleTypeByOwnerAsync(
            int ownerUserId,
            PersonType personType,
            CancellationToken ct = default)
        {
            return context.GymPerson.CountAsync(x =>
                x.IsActive &&
                x.Gym.CreatedById == ownerUserId &&
                (x.PersonType == personType || x.PersonType == PersonType.Both),
                ct);
        }


    }
}
