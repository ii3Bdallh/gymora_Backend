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
                    x.PersonType == PersonType.Owner)
                .CountAsync(ct);
        }

        public async Task<int> GetOwnerIdAsync(int gymId)
        {
            return await context.GymPerson
                .Where(x => x.GymId == gymId && x.PersonType == PersonType.Owner && x.UserId != null)
                .Select(x => x.UserId!.Value)
                .SingleAsync();
        }



        public async Task<UserGymsListRDTO> GetUserGymsAsync(
            int userId,
            UserGymsPagedReq req,
            CancellationToken cancellationToken)
        {
            //----------------------------------------------------
            // 1) Base filter: get all GymPerson rows belonging to this user
            //    (reused for both the Count query and the Page query)
            //----------------------------------------------------
            var baseQuery = context.GymPerson
                .AsNoTracking() // read-only data, so disable EF change tracking for performance
                .Where(x =>
                    x.UserId == userId);

            //----------------------------------------------------
            // 2) Get the total number of gyms for this user
            //    Executed as a pure SQL COUNT(*), no rows are loaded into memory
            //----------------------------------------------------
            var totalCount = await baseQuery.CountAsync(cancellationToken);

            // If the user has no gyms at all, return an empty result immediately
            // (avoids running all the extra queries below for nothing)
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
            // 3) Fetch the requested page of gyms (Pagination)
            //    - Skip/Take run in SQL, not in memory
            //    - An explicit OrderBy is required for stable/consistent pagination
            //----------------------------------------------------
            var gyms = await baseQuery
                .OrderBy(x => x.PersonType)   // primary sort by person type
                .ThenBy(x => x.GymId)         // secondary sort by GymId, to keep ordering stable across pages
                .Skip((req.PageNumber - 1) * req.PageSize) // skip previous pages
                .Take(req.PageSize)                        // take only this page's items
                .Select(x => new UserGymRDTO
                {
                    GymId = x.GymId,

                    GymName = x.Gym.Name,

                    LogoUrl = x.Gym.FileUrl,

                    GymPersonId = x.Id,

                    // Determine the user's role inside this gym:
                    // - Owner -> GymRole.Owner
                    // - has a StaffProfile -> use their assigned GymRoleId
                    // - otherwise -> plain Member
                    GymRole =
                        x.PersonType == PersonType.Owner
                            ? GymRole.Owner
                            : x.StaffProfile != null
                                ? x.StaffProfile.GymRoleId
                                : GymRole.Member,

                    // Membership = x.MemberProfile!.Membership, // (currently disabled/unused)

                    GymStatus = x.Gym.Status,

                    PersonAccessStatus = x.AccessStatus,

                    // Membership end date (may be null if the user isn't a Member)
                    MembershipEndDate = x.MemberProfile!.MembershipEndDate,

                })
                .ToListAsync(cancellationToken);

            //----------------------------------------------------
            // 4) Extract the GymIds for ONLY the current page
            //    (not all gyms — just the ones returned in this page)
            //----------------------------------------------------
            var gymIds = gyms
                .Select(x => x.GymId)
                .ToList();

            //----------------------------------------------------
            // 5) Get the Owners of the gyms in the current page only
            //    Build a Dictionary: GymId -> OwnerUserId
            //----------------------------------------------------
            var gymOwners = await context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    gymIds.Contains(x.GymId) &&
                    x.PersonType == PersonType.Owner &&
                    x.UserId != null)
                .ToDictionaryAsync(
                    x => x.GymId,
                    x => x.UserId!.Value,
                    cancellationToken);

            //----------------------------------------------------
            // 6) Collect the distinct Owner Ids
            //    So every downstream query targets exactly the owners
            //    that matter for this page (no wasted queries)
            //----------------------------------------------------
            var ownerIds = gymOwners.Values.Distinct().ToList();

            //----------------------------------------------------
            // 7) Number of gyms owned by each Owner
            //    (used later to compare against the Free Plan limit)
            //----------------------------------------------------
            var gymCounts = await context.Gym
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.OwnerUserId))
                .GroupBy(x => x.OwnerUserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count(),
                    cancellationToken);

            //----------------------------------------------------
            // 8) Number of Members (Member + StaffMember) across
            //    all gyms owned by each Owner
            //----------------------------------------------------
            var memberCounts = await context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.Gym.OwnerUserId) &&
                    (x.PersonType == PersonType.Member ||
                     x.PersonType == PersonType.StaffMember))
                .GroupBy(x => x.Gym.OwnerUserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count(),
                    cancellationToken);

            //----------------------------------------------------
            // 9) Number of Coaches (Staff + StaffMember) across
            //    all gyms owned by each Owner
            //----------------------------------------------------
            var coachCounts = await context.GymPerson
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.Gym.OwnerUserId) &&
                    (x.PersonType == PersonType.Staff ||
                     x.PersonType == PersonType.StaffMember))
                .GroupBy(x => x.Gym.OwnerUserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count(),
                    cancellationToken);

            //----------------------------------------------------
            // 10) Latest subscription for each Owner
            //     Group by CreatedById, order each group by EndDate
            //     descending, and pick the most recent one
            //----------------------------------------------------
            var ownerSubscriptions = await context.OwnerSubscription
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.CreatedById))
                .GroupBy(x => x.CreatedById)
                .Select(g => g
                    .OrderByDescending(x => x.EndDate)
                    .First())
                .ToDictionaryAsync(
                    x => x.CreatedById,
                    cancellationToken);

            //----------------------------------------------------
            // 11) Load the Free Plan limits
            //     Used as a reference point, e.g. for owners without
            //     an active paid subscription
            //----------------------------------------------------
            SubscriptionPlan? freePlan = await context.SubscriptionPlan
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsFree, cancellationToken);

            // If no Free Plan is configured in the system at all, that's an
            // unexpected/invalid state -> throw
            if (freePlan is null)
                throw new NotFoundException("Free subscription plan not found.");

            int maxGyms = freePlan.MaxOwnedGyms;     // max gyms allowed on the Free Plan

            int maxMembers = freePlan.MaxMembers;    // max members allowed

            int maxCoaches = freePlan.MaxCoaches;    // max coaches allowed

            //----------------------------------------------------
            // 12) Check whether the CURRENT requesting user has an
            //     active subscription right now (EndDate hasn't passed yet)
            //----------------------------------------------------
            var now = DateTime.UtcNow;

            var hasActivePlatformSubscription = await context.OwnerSubscription
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CreatedById == userId &&
                    now <= x.EndDate,
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


                    }
                }

                // throw new Exception("PersonAccessStatus is not Active");

                //----------------------------------------------------
                // Membership (kept for future use, as in the original)
                //----------------------------------------------------

                if (isAccessible &&
                    item.HasActiveMembership)
                {
                    switch (item.HasActiveMembership)
                    {
                        case true:
                            break;
                        case false:
                            accessStatus = GymAccessStatus.MembershipExpired;
                            isAccessible = false;
                            inaccessibleReason = "Your membership has expired.";
                            break;
                    }
                }

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