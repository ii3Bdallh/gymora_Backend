using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Model;
using Domain.Enum;
using Domain.Model;
using Gymora.Contracts.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repo
{
    public class GymAccessRepo(ApplicationDbContext context) : IGymAccessRepo
    {
        // public Task<IReadOnlyList<AvailableGymDto>> GetAvailableGymsAsync(int userId, CancellationToken ct = default)
        // {
        //     throw new NotImplementedException();
        // }

        public async Task<MyGymDto?> GetGymAccessAsync(
      int userId,
      int gymId,
      CancellationToken ct = default)
        {
            // ===========================
            // Owner
            // ===========================

            var ownerPerson = await context.GymPerson
                .AsNoTracking()
                .Include(x => x.Gym)
                .Where(x =>
                    x.GymId == gymId &&
                    x.UserId == userId &&
                    x.PersonType == PersonType.Owner &&
                    x.IsActive)
                .FirstOrDefaultAsync(ct);

            if (ownerPerson != null)
            {
                ValidateGymAccess(ownerPerson.Gym);

                await ValidateOwnerSubscriptionAsync(userId, ct);

                return new MyGymDto
                {
                    GymPeopleId = ownerPerson.Id,
                    GymId = ownerPerson.Gym.Id,
                    GymName = ownerPerson.Gym.Name,
                    GymRole = GymRole.Owner.ToRoleString()
                };
            }

            // ===========================
            // Staff / Member
            // ===========================

            var gymPerson = await context.GymPerson
                .AsNoTracking()
                .Include(x => x.Gym)
                .Include(x => x.StaffProfile)
                .Include(x => x.MemberProfile)
                // .ThenInclude(x => x.Membership)
                .Where(x =>
                    x.UserId == userId &&
                    x.GymId == gymId &&
                    x.IsActive)
                .FirstOrDefaultAsync(ct);

            if (gymPerson == null)
                return null;

            ValidateGymAccess(gymPerson.Gym);

            ValidateGymPersonAccessStatus(gymPerson.AccessStatus);

            var ownerPersonId = await context.GymPerson
                .AsNoTracking()
                .Where(x => x.GymId == gymPerson.GymId && x.PersonType == PersonType.Owner && x.IsActive)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync(ct);

            if (ownerPersonId == null)
                throw new ForbiddenException("No active owner found for this gym.");

            await ValidateOwnerSubscriptionAsync(ownerPersonId.Value, ct);

            // if (gymPerson.PersonType == PersonType.Member ||
            //     gymPerson.PersonType == PersonType.StaffMember)
            // {
            //     ValidateMembership(gymPerson.MemberProfile?.Membership);
            // }

            return new MyGymDto
            {
                GymPeopleId = gymPerson.Id,
                GymId = gymPerson.GymId,
                GymName = gymPerson.Gym.Name,
                GymRole = gymPerson.StaffProfile != null
                    ? gymPerson.StaffProfile.GymRoleId.ToString()
                    : GymRole.Other.ToString()
            };
        }

        private static void ValidateGymAccess(Gym gym)
        {
            if (gym.Status == GymStatus.Suspended)
                throw new ForbiddenException("This gym has been suspended.");
        }

        private static void ValidateGymPersonAccessStatus(GymPersonAccessStatus status)
        {
            switch (status)
            {
                case GymPersonAccessStatus.Active:
                    return;

                case GymPersonAccessStatus.Suspended:
                    throw new ForbiddenException("Gym person access is suspended.");

                case GymPersonAccessStatus.Blocked:
                    throw new ForbiddenException("Gym person access is blocked.");

                case GymPersonAccessStatus.LeftGym:
                    throw new ForbiddenException("Gym person access is left gym.");

                default:
                    throw new ForbiddenException("Gym person access is not active.");
            }
        }


        private async Task ValidateOwnerSubscriptionAsync(
    int ownerId,
    CancellationToken ct)
        {
            var subscription = await context.OwnerSubscription
                .Where(x =>
                    x.CreatedById == ownerId &&
                    x.IsActive)
                .OrderByDescending(x => x.EndDate)
                .FirstOrDefaultAsync(ct);

            if (subscription == null)
                throw new ForbiddenException("No active subscription found.");

            switch (subscription.Status)
            {
                case OwnerSubscriptionStatus.Active:
                case OwnerSubscriptionStatus.Grace:
                    return;

                case OwnerSubscriptionStatus.Expired:
                    throw new ForbiddenException("Owner subscription has expired.");

                case OwnerSubscriptionStatus.Suspended:
                    throw new ForbiddenException("Owner subscription is suspended.");

                default:
                    throw new ForbiddenException("Subscription is not available.");
            }
        }

        // private static void ValidateMembership(Membership? membership)
        // {
        //     if (membership == null)
        //         throw new ForbiddenException("Membership not found.");

        //     switch (membership.Status)
        //     {
        //         case MembershipStatus.Active:
        //             return;

        //         case MembershipStatus.Expired:
        //             throw new ForbiddenException("Your membership has expired.");

        //         case MembershipStatus.Frozen:
        //             throw new ForbiddenException("Your membership is frozen.");

        //         case MembershipStatus.Suspended:
        //             throw new ForbiddenException("Your membership is suspended.");

        //         default:
        //             throw new ForbiddenException("Membership is not active.");
        //     }
        // }







    }
}