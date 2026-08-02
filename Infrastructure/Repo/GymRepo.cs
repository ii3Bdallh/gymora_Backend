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

namespace Infrastructure.Repo
{
    public class GymRepo(ApplicationDbContext context, ILogger<GymRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseAuditableRepo<Gym>(context, logger, queryCache, currentUser), IGymRepo
    {

        public async Task<int> CountOwnedByOwnerAsync(
    int ownerUserId,
    CancellationToken ct = default)
        {
            return await DbSet
                .AsNoTracking()
                .Where(x =>
                    x.CreatedById == ownerUserId &&
                    x.IsActive)
                .CountAsync(ct);
        }

        public async Task<int> GetOwnerIdAsync(int gymId)
        {
            return await DbSet
                .Where(x => x.Id == gymId)
                .Select(x => x.CreatedById)
                .SingleAsync();
        }



        public async Task<UserGymsListRDTO> GetUserGymsAsync(
            int userId,
            UserGymsPagedReq req,
            CancellationToken cancellationToken)
        {
            //----------------------------------------------------
            // Owner Gyms
            //----------------------------------------------------

            var ownerGyms = await context.Gym
                .AsNoTracking()
                .Where(x =>
                    x.CreatedById == userId &&
                    x.IsActive)
                .Select(x => new UserGymAccessItem
                {
                    IsOwner = true,

                    Gym = x,

                    GymPersonId = null,

                    PersonType = null,

                    GymRole = GymRole.Owner,

                    PersonAccessStatus = (GymPersonAccessStatus?)null,

                    // MembershipStatus = (MembershipStatus?)null
                })
                .ToListAsync(cancellationToken);

            //----------------------------------------------------
            // Staff + Member + Both
            //----------------------------------------------------

            var peopleGyms = await context.GymPerson
                .AsNoTracking()
                .Include(x => x.Gym)
                .Include(x => x.StaffProfile)
                .Include(x => x.MemberProfile)
                // .ThenInclude(x => x.Membership)
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive)
                .Select(x => new UserGymAccessItem
                {
                    IsOwner = false,

                    Gym = x.Gym,

                    GymPersonId = x.Id,

                    PersonType = x.PersonType,

                    GymRole =
                        x.StaffProfile != null
                            ? x.StaffProfile.GymRoleId
                            : GymRole.Member,

                    PersonAccessStatus = x.AccessStatus,

                    // // MembershipStatus =
                    // //     x.MemberProfile != null &&
                    // //     x.MemberProfile.Membership != null
                    // //         ? x.MemberProfile.Membership.Status
                    // //         : (MembershipStatus?)null
                })
                .ToListAsync(cancellationToken);

            //----------------------------------------------------
            // Merge
            //----------------------------------------------------

            var gyms = ownerGyms
                .Concat(peopleGyms)
                .GroupBy(x => x.Gym.Id)
                .Select(x => x.First())
                .ToList();

            if (gyms.Count == 0)
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
            // Collect Ids
            //----------------------------------------------------

            var ownerIds = gyms
                .Select(x => x.Gym.CreatedById)
                .Distinct()
                .ToList();

            var gymIds = gyms
                .Select(x => x.Gym.Id)
                .ToList();
            //----------------------------------------------------
            // Latest Subscription For Each Owner
            //----------------------------------------------------

            var subscriptions = await context.OwnerSubscription
                .AsNoTracking()
                .Where(x =>
                    ownerIds.Contains(x.CreatedById) &&
                    x.IsActive)
                .OrderByDescending(x => x.EndDate)
                .ToListAsync(cancellationToken);

            var ownerSubscriptions = subscriptions
                .GroupBy(x => x.CreatedById)
                .ToDictionary(
                    x => x.Key,
                    x => x.First());

            //----------------------------------------------------
            // Free Plan
            //----------------------------------------------------

            var freePlan = await context.SubscriptionPlan
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.IsFree && x.IsActive,
                    cancellationToken);

            //----------------------------------------------------
            // Current User Platform Subscription
            //----------------------------------------------------

            var hasActivePlatformSubscription = await context.OwnerSubscription
                .AsNoTracking()
                .AnyAsync(x =>
                    x.CreatedById == userId &&
                    x.IsActive &&
                    (
                        x.Status == OwnerSubscriptionStatus.Active ||
                        x.Status == OwnerSubscriptionStatus.Grace
                    ),
                    cancellationToken);

            //----------------------------------------------------
            // Build Result
            //----------------------------------------------------

            var result = new List<UserGymRDTO>();

            foreach (var item in gyms)
            {
                var gym = item.Gym;

                var role = item.GymRole.ToString();

                var accessStatus = GymAccessStatus.Active;

                var isAccessible = true;

                string? inaccessibleReason = null;

                //----------------------------------------------------
                // Gym Validation
                //----------------------------------------------------

                if (gym.Status == GymStatus.Suspended)
                {
                    accessStatus = GymAccessStatus.GymSuspended;
                    isAccessible = false;
                    inaccessibleReason = "This gym has been suspended.";
                }

                //----------------------------------------------------
                // Owner Subscription
                //----------------------------------------------------

                else if (ownerSubscriptions.TryGetValue(gym.CreatedById, out var subscription))
                {
                    switch (subscription.Status)
                    {
                        case OwnerSubscriptionStatus.Active:
                            break;

                        case OwnerSubscriptionStatus.Grace:
                            accessStatus = GymAccessStatus.OwnerSubscriptionGrace;
                            break;

                        case OwnerSubscriptionStatus.Expired:
                            accessStatus = GymAccessStatus.OwnerSubscriptionExpired;
                            isAccessible = false;
                            inaccessibleReason = "The owner's subscription has expired.";
                            break;

                        case OwnerSubscriptionStatus.Suspended:
                            accessStatus = GymAccessStatus.OwnerSubscriptionSuspended;
                            isAccessible = false;
                            inaccessibleReason = "The owner's subscription is suspended.";
                            break;
                    }
                }

                //----------------------------------------------------
                // Gym Person Access
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
                            inaccessibleReason = "Your access to this gym is suspended.";
                            break;

                        case GymPersonAccessStatus.Blocked:
                            accessStatus = GymAccessStatus.PersonBlocked;
                            isAccessible = false;
                            inaccessibleReason = "You have been blocked from this gym.";
                            break;

                        case GymPersonAccessStatus.LeftGym:
                            accessStatus = GymAccessStatus.LeftGym;
                            isAccessible = false;
                            inaccessibleReason = "You are no longer a member of this gym.";
                            break;
                    }
                }

                //----------------------------------------------------
                // Membership
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
                    GymPeopleId = item.GymPersonId ?? 0,

                    GymId = gym.Id.ToString(),

                    GymName = gym.Name,

                    LogoUrl = gym.FileUrl,

                    Role = role,

                    GymAccessStatus = accessStatus,

                    IsAccessible = isAccessible,

                    InaccessibleReason = inaccessibleReason
                });
            }

            //----------------------------------------------------
            // Pagination
            //----------------------------------------------------

            var totalCount = result.Count;

            var paginated = result
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            //----------------------------------------------------
            // Response
            //----------------------------------------------------

            return new UserGymsListRDTO
            {
                Gyms = paginated,

                HasActivePlatformSubscription = hasActivePlatformSubscription,

                TotalCount = totalCount,

                PageNumber = req.PageNumber,

                PageSize = req.PageSize
            };

        }
    }
}