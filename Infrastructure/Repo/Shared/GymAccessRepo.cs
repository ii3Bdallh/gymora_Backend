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
        
        public async Task<bool> CanJoinGymAsync(int gymId, PersonType personType, CancellationToken ct = default)
        {
            var ownerPerson = await context.GymPerson
                .AsNoTracking()
                .Where(x => x.GymId == gymId && x.PersonType == PersonType.Owner)
                .FirstOrDefaultAsync(ct);

            if (ownerPerson == null || ownerPerson.UserId == null)
                return false;

            var currentPlan = await currentPlanService.GetCurrentPlanAsync(ownerPerson.UserId.Value, ct);
            if (currentPlan == null)
                return false;

            if (currentPlan.SubscriptionStatus != OwnerSubscriptionStatus.Active)
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
                    x.PersonType == PersonType.Owner)
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
                    x.GymId == gymId)
                .FirstOrDefaultAsync(ct);

            if (gymPerson == null)
                return null;

            ValidateGymAccess(gymPerson.Gym);

            ValidateGymPersonAccessStatus(gymPerson.AccessStatus);

            var ownerPersonId = await context.GymPerson
                .AsNoTracking()
                .Where(x => x.GymId == gymPerson.GymId && x.PersonType == PersonType.Owner)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync(ct);

            if (ownerPersonId == null)
                throw new ForbiddenException("No active owner found for this gym.");

            await ValidateOwnerSubscriptionAsync(ownerPersonId.Value, ct);

            if (gymPerson.PersonType == PersonType.Member)
            {
                ValidateMembership(gymPerson.MemberProfile?.MembershipEndDate > DateTime.UtcNow);
            }
            else if (gymPerson.PersonType == PersonType.Staff)
            {
                ValidateSalary(gymPerson.StaffProfile?.SalaryValidUntil > DateTime.UtcNow);
            }
            else if (gymPerson.PersonType == PersonType.StaffMember)
            {
                ValidateMembershipAndSalary(
                    gymPerson.MemberProfile?.MembershipEndDate > DateTime.UtcNow,
                    gymPerson.StaffProfile?.SalaryValidUntil > DateTime.UtcNow);
            }

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
                    return;

                case OwnerSubscriptionStatus.Expired:
                    throw new ForbiddenException("Owner subscription has expired.");

                case OwnerSubscriptionStatus.Suspended:
                    throw new ForbiddenException("Owner subscription is suspended.");

                default:
                    throw new ForbiddenException("Subscription is not available.");
            }
        }

        private static void ValidateMembership(bool HasActiveMembership)
        {
            switch (HasActiveMembership)
            {
                case true:
                    return;

                case false:
                    throw new ForbiddenException("Your membership has expired.");
            }
        }

        private static void ValidateSalary(bool isSalaryPaid)
        {
            if (!isSalaryPaid)
                throw new ForbiddenException("Your staff salary payment period has expired or has not been paid.");
        }

        private static void ValidateMembershipAndSalary(bool hasActiveMembership, bool isSalaryPaid)
        {
            if (!hasActiveMembership && !isSalaryPaid)
                throw new ForbiddenException("Both your membership and staff salary payment period have expired.");

            if (!hasActiveMembership)
                throw new ForbiddenException("Your membership has expired.");

            if (!isSalaryPaid)
                throw new ForbiddenException("Your staff salary payment period has expired or has not been paid.");
        }
    }
}