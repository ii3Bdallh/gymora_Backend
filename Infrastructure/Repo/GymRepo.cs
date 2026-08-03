using Application.Interface.Repo;
using Domain.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Repo.Base;
using Domain.Model.Auth;
using Google;
using Infrastructure.Persistence;
using Infrastructure.Cache;
using Application.Model;
using Microsoft.EntityFrameworkCore;
using Gymora.Contracts.Authentication;
using Domain.Enum;
using Application.DTO.Model;
using Application.DTO.Exceptions;

namespace Infrastructure.Repo
{
    public class GymRepo(ApplicationDbContext context, ILogger<GymRepo> logger, QueryCache queryCache)
    : BaseRepo<Gym>(context, logger, queryCache), IGymRepo
    {

        public async Task<int> CountOwnedByOwnerAsync(
    int ownerUserId,
    CancellationToken ct = default)
        {
            return await context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    x.UserId == ownerUserId &&
                    x.PersonType == PersonType.Owner &&
                    x.IsActive)
                .CountAsync(ct);
        }

        public async Task<int> GetOwnerIdAsync(int gymId)
        {
            return await context.GymPerson
                .Where(x => x.GymId == gymId && x.PersonType == PersonType.Owner && x.IsActive && x.UserId != null)
                .Select(x => x.UserId!.Value)
                .SingleAsync();
        }



        public async Task<UserGymsListRDTO> GetUserGymsAsync(
            int userId,
            UserGymsPagedReq req,
            CancellationToken cancellationToken)
        {
            //----------------------------------------------------
            // Base filter (shared by Count + Page queries)
            //----------------------------------------------------

            var baseQuery = context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive &&
                    x.Gym.IsActive);

            //----------------------------------------------------
            // Total Count (pure SQL COUNT, no rows loaded)
            //----------------------------------------------------

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new UserGymsListRDTO
                {
                    Gyms = [],
                    TotalCount = 0,
                    PageNumber = req.PageNumber,
                    PageSize = req.PageSize,
                    HasActivePlatformSubscription = false
                };
            }

            //----------------------------------------------------
            // Get User Gyms — Skip/Take executed in SQL, with an
            // explicit ordering (required for stable pagination)
            //----------------------------------------------------

            var gyms = await baseQuery
                .OrderBy(x => x.Gym.Name)
                .ThenBy(x => x.GymId)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .Select(x => new UserGymRDTO
                {
                    GymId = x.GymId,

                    GymName = x.Gym.Name,

                    LogoUrl = x.Gym.FileUrl,

                    GymPersonId = x.Id,

                    GymRole =
                        x.PersonType == PersonType.Owner
                            ? GymRole.Owner
                            : x.StaffProfile != null
                                ? x.StaffProfile.GymRoleId
                                : GymRole.Member,

                    // Membership = x.MemberProfile!.Membership,

                    GymStatus = x.Gym.Status,

                    PersonAccessStatus = x.AccessStatus
                })
                .ToListAsync(cancellationToken);

            //----------------------------------------------------
            // Gym Ids — only for the current page
            //----------------------------------------------------

            var gymIds = gyms
                .Select(x => x.GymId)
                .ToList();

            //----------------------------------------------------
            // Get Gym Owners — only for gyms in the current page
            //----------------------------------------------------

            var gymOwners = await context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    gymIds.Contains(x.GymId) &&
                    x.PersonType == PersonType.Owner &&
                    x.IsActive &&
                    x.UserId != null)
                .ToDictionaryAsync(
                    x => x.GymId,
                    x => x.UserId!.Value,
                    cancellationToken);

            //----------------------------------------------------
            // Collect Owner Ids — derived from verified owners only,
            // so every downstream query targets exactly the owners
            // that matter for this page
            //----------------------------------------------------

            var ownerIds = gymOwners.Values.Distinct().ToList();

            var gymCounts = await context.Gym
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.OwnerUserId) &&
                    x.IsActive)
                .GroupBy(x => x.OwnerUserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count(),
                    cancellationToken);

            var memberCounts = await context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.Gym.OwnerUserId) &&
                    x.IsActive &&
                    (x.PersonType == PersonType.Member ||
                     x.PersonType == PersonType.StaffMember))
                .GroupBy(x => x.Gym.OwnerUserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count(),
                    cancellationToken);

            var coachCounts = await context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.Gym.OwnerUserId) &&
                    x.IsActive &&
                    (x.PersonType == PersonType.Staff ||
                     x.PersonType == PersonType.StaffMember))
                .GroupBy(x => x.Gym.OwnerUserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count(),
                    cancellationToken);

            //----------------------------------------------------
            // Latest Subscription For Each Owner
            //----------------------------------------------------

            var ownerSubscriptions = await context.OwnerSubscription
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.CreatedById) &&
                    x.IsActive)
                .GroupBy(x => x.CreatedById)
                .Select(g => g
                    .OrderByDescending(x => x.EndDate)
                    .First())
                .ToDictionaryAsync(
                    x => x.CreatedById,
                    cancellationToken);

            //----------------------------------------------------
            // Free Plan limits
            //----------------------------------------------------

            SubscriptionPlan? freePlan = await context.SubscriptionPlan
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive && x.IsFree, cancellationToken);

            if (freePlan is null)
                throw new NotFoundException("Free subscription plan not found.");

            int maxGyms = freePlan.MaxOwnedGyms;

            int maxMembers = freePlan.MaxMembers;

            int maxCoaches = freePlan.MaxCoaches;

            //----------------------------------------------------
            // Current User Platform Subscription
            //----------------------------------------------------

            var now = DateTime.UtcNow;

            var hasActivePlatformSubscription = await context.OwnerSubscription
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CreatedById == userId &&
                    x.IsActive &&
                    now <= x.GraceEndDate,
                    cancellationToken);

            //----------------------------------------------------
            // Build Result — only for the already-paginated page
            //----------------------------------------------------

            var result = new List<UserGymRDTO>(gyms.Count);

            foreach (var item in gyms)
            {
                GymAccessStatus accessStatus = GymAccessStatus.Active;

                var isAccessible = true;

                string? inaccessibleReason = null;

                //----------------------------------------------------
                // Gym Status
                //----------------------------------------------------

                if (item.GymStatus == GymStatus.Suspended)
                {
                    accessStatus = GymAccessStatus.GymSuspended;
                    isAccessible = false;
                    inaccessibleReason = "This gym has been suspended.";
                }

                //----------------------------------------------------
                // Owner Subscription
                //----------------------------------------------------

                if (isAccessible)
                {
                    if (!gymOwners.TryGetValue(item.GymId, out var ownerUserId))
                    {
                        accessStatus = GymAccessStatus.OwnerNotFound;
                        isAccessible = false;
                        inaccessibleReason = "Gym owner not found.";
                    }
                    else
                    {
                        var currentMembers = memberCounts.GetValueOrDefault(ownerUserId);
                        var currentCoaches = coachCounts.GetValueOrDefault(ownerUserId);
                        var currentGyms = gymCounts.GetValueOrDefault(ownerUserId);

                        var isOverFreeLimit =
                            currentMembers > maxMembers ||
                            currentCoaches > maxCoaches ||
                            currentGyms > maxGyms;

                        if (ownerSubscriptions.TryGetValue(ownerUserId, out var subscription))
                        {
                            switch (subscription.Status)
                            {
                                case OwnerSubscriptionStatus.Active:
                                    break;

                                case OwnerSubscriptionStatus.Grace:
                                    accessStatus = GymAccessStatus.OwnerSubscriptionGrace;
                                    break;

                                case OwnerSubscriptionStatus.Expired:

                                    // Expired paid subscription -> owner falls back to the Free Plan
                                    if (isOverFreeLimit)
                                    {
                                        accessStatus = GymAccessStatus.OwnerPlanLimitReached;
                                        isAccessible = false;
                                        inaccessibleReason = "Owner plan limit reached.";
                                    }

                                    break;

                                case OwnerSubscriptionStatus.Suspended:
                                    accessStatus = GymAccessStatus.OwnerSubscriptionSuspended;
                                    isAccessible = false;
                                    inaccessibleReason = "Owner subscription is suspended.";
                                    break;
                            }
                        }
                        else
                        {
                            // Owner never had a subscription at all -> also on the Free Plan.
                            // (Previously this branch was skipped entirely and limits were never checked.)
                            if (isOverFreeLimit)
                            {
                                accessStatus = GymAccessStatus.OwnerPlanLimitReached;
                                isAccessible = false;
                                inaccessibleReason = "Owner plan limit reached.";
                            }
                        }
                    }
                }

                //----------------------------------------------------
                // Person Access
                //----------------------------------------------------

                if (isAccessible &&
                    item.PersonAccessStatus.HasValue)
                {
                    switch (item.PersonAccessStatus.Value)
                    {
                        case GymPersonAccessStatus.Active:
                            break;

                        case GymPersonAccessStatus.Suspended:
                            accessStatus = GymAccessStatus.PersonSuspended;
                            isAccessible = false;
                            inaccessibleReason = "Your access has been suspended.";
                            break;

                        case GymPersonAccessStatus.Blocked:
                            accessStatus = GymAccessStatus.PersonBlocked;
                            isAccessible = false;
                            inaccessibleReason = "You have been blocked.";
                            break;

                        case GymPersonAccessStatus.LeftGym:
                            accessStatus = GymAccessStatus.LeftGym;
                            isAccessible = false;
                            inaccessibleReason = "You are no longer part of this gym.";
                            break;
                    }
                }

                //----------------------------------------------------
                // Membership (kept for future use, as in the original)
                //----------------------------------------------------

                // if (isAccessible &&
                //     item.Membership != null)
                // {
                //     switch (item.Membership.Status)
                //     {
                //         case MembershipStatus.Active:
                //             break;
                //         case MembershipStatus.Grace:
                //             accessStatus = GymAccessStatus.MembershipGrace;
                //             break;
                //         case MembershipStatus.Expired:
                //             accessStatus = GymAccessStatus.MembershipExpired;
                //             isAccessible = false;
                //             inaccessibleReason = "Your membership has expired.";
                //             break;
                //         case MembershipStatus.Frozen:
                //             accessStatus = GymAccessStatus.MembershipFrozen;
                //             isAccessible = false;
                //             inaccessibleReason = "Your membership is frozen.";
                //             break;
                //         case MembershipStatus.Cancelled:
                //             accessStatus = GymAccessStatus.MembershipCancelled;
                //             isAccessible = false;
                //             inaccessibleReason = "Your membership has been cancelled.";
                //             break;
                //     }
                // }

                result.Add(new UserGymRDTO
                {
                    GymPersonId = item.GymPersonId,

                    GymId = item.GymId,

                    GymName = item.GymName,

                    LogoUrl = item.LogoUrl,

                    GymRole = item.GymRole,

                    GymAccessStatus = accessStatus,

                    GymStatus = item.GymStatus,

                    PersonAccessStatus = item.PersonAccessStatus,

                    IsAccessible = isAccessible,

                    InaccessibleReason = inaccessibleReason
                });
            }

            //----------------------------------------------------
            // Return
            //----------------------------------------------------

            return new UserGymsListRDTO
            {
                Gyms = result,
                HasActivePlatformSubscription = hasActivePlatformSubscription,
                TotalCount = totalCount,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize
            };
        }
    }
}