using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service.Shared;
using Application.Model;
using Domain.Enum;
using Domain.Model;
using Gymora.Contracts.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repo
{
    public class GymAccessRepo(ApplicationDbContext context, ICurrentPlanService currentPlanService) : IGymAccessRepo
    {
        // public async Task<IReadOnlyList<AvailableGymDto>> GetAvailableGymsAsync(int userId, CancellationToken ct = default)
        // {
        //     var gymPeople = await context.GymPerson
        //         .AsNoTracking()
        //         .Include(x => x.Gym)
        //         .Include(x => x.StaffProfile)
        //         .Include(x => x.MemberProfile)
        //         .Where(x => x.UserId == userId && x.IsActive)
        //         .ToListAsync(ct);

        //     var availableGyms = new List<AvailableGymDto>();

        //     foreach (var gp in gymPeople)
        //     {
        //         if (gp.Gym.Status == GymStatus.Suspended)
        //             continue;

        //         if (gp.AccessStatus != GymPersonAccessStatus.Active)
        //             continue;

        //         int ownerId;
        //         if (gp.PersonType == PersonType.Owner)
        //         {
        //             ownerId = userId;
        //         }
        //         else
        //         {
        //             var ownerUserId = await context.GymPerson
        //                 .AsNoTracking()
        //                 .Where(x => x.GymId == gp.GymId && x.PersonType == PersonType.Owner && x.IsActive)
        //                 .Select(x => x.UserId)
        //                 .FirstOrDefaultAsync(ct);

        //             if (ownerUserId == null)
        //                 continue;

        //             ownerId = ownerUserId.Value;
        //         }

        //         try
        //         {
        //             var subscription = await currentPlanService.GetCurrentPlanAsync(ownerId, ct);
        //             if (subscription == null || subscription.IsCompliant == false)
        //                 continue;

        //             if (subscription.SubscriptionStatus != OwnerSubscriptionStatus.Active &&
        //                 subscription.SubscriptionStatus != OwnerSubscriptionStatus.Grace)
        //             {
        //                 continue;
        //             }
        //         }
        //         catch
        //         {
        //             continue;
        //         }

        //         string role = gp.PersonType == PersonType.Owner
        //             ? GymRole.Owner.ToRoleString()
        //             : (gp.StaffProfile != null ? gp.StaffProfile.GymRoleId.ToString() : GymRole.Other.ToString());

        //         availableGyms.Add(new AvailableGymDto
        //         {
        //             GymId = gp.GymId.ToString(),
        //             GymName = gp.Gym.Name,
        //             Role = role
        //         });
        //     }

        //     return availableGyms;
        // }

        public async Task<bool> CanJoinGymAsync(int gymId, PersonType personType, CancellationToken ct = default)
        {
            var ownerPerson = await context.GymPerson
                .AsNoTracking()
                .Where(x => x.GymId == gymId && x.PersonType == PersonType.Owner && x.IsActive)
                .FirstOrDefaultAsync(ct);

            if (ownerPerson == null || ownerPerson.UserId == null)
                return false;

            var currentPlan = await currentPlanService.GetCurrentPlanAsync(ownerPerson.UserId.Value, ct);
            if (currentPlan == null)
                return false;

            if (currentPlan.SubscriptionStatus != OwnerSubscriptionStatus.Active &&
                currentPlan.SubscriptionStatus != OwnerSubscriptionStatus.Grace)
            {
                return false;
            }

            if (personType == PersonType.Member)
            {
                return currentPlan.CurrentMemberCount < currentPlan.MaxMembers;
            }
            else if (personType == PersonType.Staff)
            {
                return currentPlan.CurrentCoachCount < currentPlan.MaxCoaches;
            }

            return true;
        }

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
            CurrentPlanResult subscription = await currentPlanService.GetCurrentPlanAsync(ownerId);

            if (subscription == null)
                throw new ForbiddenException("No active subscription found.");

            if (subscription.IsCompliant == false)
                throw new ForbiddenException("Owner subscription is not compliant.");

            switch (subscription.SubscriptionStatus)
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