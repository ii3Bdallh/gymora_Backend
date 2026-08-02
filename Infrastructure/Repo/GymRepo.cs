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



        public async Task<UserGymsListRDTO> GetUserGymsAsync(int userId, UserGymsPagedReq req, CancellationToken cancellationToken)
        {
            var ownerGymsQuery = context.Gym
                .Where(x => x.CreatedById == userId && x.IsActive);

            var staffGymsQuery = context.GymPerson
                .Include(x => x.Gym)
                .Where(x => x.UserId == userId && x.IsActive && (x.PersonType == PersonType.Staff || x.PersonType == PersonType.Both))
                .Select(x => x.Gym);

            var combinedGymsQuery = ownerGymsQuery.Union(staffGymsQuery);

            if (!string.IsNullOrEmpty(req.SearchTerm))
            {
                combinedGymsQuery = combinedGymsQuery.Where(x => x.Name.Contains(req.SearchTerm));
            }

            var gyms = await combinedGymsQuery.ToListAsync(cancellationToken);
            var resultList = new List<UserGymRDTO>();

            foreach (var gym in gyms)
            {
                string role = GymRole.Member.ToString();
                if (gym.CreatedById == userId)
                {
                    role = GymRole.Owner.ToString();
                }
                else
                {
                    var staff = await context.GymPerson
                        .Include(x => x.StaffProfile)
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.GymId == gym.Id && x.IsActive && (x.PersonType == PersonType.Staff || x.PersonType == PersonType.Both), cancellationToken);
                    if (staff?.StaffProfile != null)
                    {
                        role = staff.StaffProfile.GymRoleId.ToString();
                    }
                }

                string status = "Active";
                bool isAccessible = true;
                string? inaccessibleReason = null;

                if (gym.Status == GymStatus.Suspended)
                {
                    status = "Blocked";
                    isAccessible = false;
                    inaccessibleReason = "Gym is blocked by administrator.";
                }
                else
                {
                    var ownerSub = await context.OwnerSubscription
                        .Include(x => x.Plan)
                        .Where(x => x.CreatedById == gym.CreatedById && x.IsActive)
                        .OrderByDescending(x => x.EndDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (ownerSub == null)
                    {
                        var freePlan = await context.SubscriptionPlan.FirstOrDefaultAsync(x => x.IsFree && x.IsActive == true, cancellationToken);
                        if (freePlan == null)
                        {
                            status = "Locked";
                            isAccessible = false;
                            inaccessibleReason = "Owner subscription expired. Gym access is restricted.";
                        }
                    }
                    else if (ownerSub.Status == OwnerSubscriptionStatus.Expired)
                    {
                        status = "Locked";
                        isAccessible = false;
                        inaccessibleReason = "Owner subscription expired. Gym access is restricted.";
                    }
                    else if (ownerSub.Status == OwnerSubscriptionStatus.Grace)
                    {
                        status = "Expired";
                    }
                }

                resultList.Add(new UserGymRDTO
                {
                    GymId = gym.Id.ToString(),
                    GymName = gym.Name,
                    LogoUrl = gym.FileUrl,
                    Role = role,
                    GymStatus = status,
                    IsAccessible = isAccessible,
                    InaccessibleReason = inaccessibleReason
                });
            }

            if (!string.IsNullOrEmpty(req.StatusFilter))
            {
                resultList = resultList.Where(x => x.GymStatus.Equals(req.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(req.OrderBy))
            {
                if (req.OrderBy.Equals("GymName", StringComparison.OrdinalIgnoreCase))
                {
                    resultList = req.OrderDirection == "asc"
                        ? resultList.OrderBy(x => x.GymName).ToList()
                        : resultList.OrderByDescending(x => x.GymName).ToList();
                }
                if (req.OrderBy.Equals("Role", StringComparison.OrdinalIgnoreCase))
                {
                    resultList = req.OrderDirection == "asc"
                        ? resultList.OrderBy(x => x.Role).ToList()
                        : resultList.OrderByDescending(x => x.Role).ToList();
                }
                if (req.OrderBy.Equals("GymStatus", StringComparison.OrdinalIgnoreCase))
                {
                    resultList = req.OrderDirection == "asc"
                        ? resultList.OrderBy(x => x.GymStatus).ToList()
                        : resultList.OrderByDescending(x => x.GymStatus).ToList();
                }
            }

            var totalCount = resultList.Count;

            var paginated = resultList
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            var ownerHasActiveSub = await context.OwnerSubscription
                .AnyAsync(x => x.CreatedById == userId && x.IsActive &&
                    (x.EndDate >= DateTime.UtcNow || x.GraceEndDate >= DateTime.UtcNow), cancellationToken);

            return new UserGymsListRDTO
            {
                Gyms = paginated,
                HasActivePlatformSubscription = ownerHasActiveSub,
                TotalCount = totalCount,
                PageNumber = req.PageNumber,
                PageSize = req.PageSize
            };
        }
    }
}