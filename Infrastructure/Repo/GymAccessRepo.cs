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
            // Owner
            var ownerGym = await context.Gym
                .AsNoTracking()
                .Where(x =>
                    x.CreatedById == userId &&
                    x.Id == gymId &&
                    x.IsActive)
                .Select(x => new MyGymDto
                {
                    GymId = x.Id,
                    GymName = x.Name,
                    GymRole = GymRole.Owner.ToRoleString(),
                })
                .FirstOrDefaultAsync(ct);

            if (ownerGym is not null)
                return ownerGym;

            // Staff
            var staffGym = await context.GymStaff
                .AsNoTracking()
                .Include(x => x.Gym)
                .Where(x =>
                    x.UserId == userId &&
                    x.GymId == gymId &&
                    x.IsActive)
                .Select(x => new MyGymDto
                {
                    GymPeopleId = x.Id,
                    GymId = x.GymId,
                    GymName = x.Gym.Name,
                    GymRole = x.GymRole.ToString(),
                })
                .FirstOrDefaultAsync(ct);

            if (staffGym is not null)
                return staffGym;


            return null;
        }

        //         public async Task<List<MyGymDto>> GetMyGymsAsync(
        //             int userId,
        //             CancellationToken ct = default)
        //         {
        //             var ownerGyms = context.Gym
        //                 .AsNoTracking()
        //                 .Where(x => x.CreatedById == userId && x.IsActive)
        //                 .Select(x => new MyGymDto
        //                 {
        //                     GymId = x.Id,
        //                     GymName = x.Name,
        //                     GymRole = GymRole.Owner.ToRoleString()
        //                 });

        //             var staffGyms = context.GymStaff
        //                 .AsNoTracking()
        //                 .Where(x => x.UserId == userId && x.IsActive)
        //                 .Select(x => new MyGymDto
        //                 {
        //                     GymId = x.GymId,
        //                     GymName = x.Gym.Name,
        //                     GymRole = x.GymRole.ToString()
        //                 });

        //             var gyms = await ownerGyms
        //                 .Union(staffGyms)
        //                 .OrderBy(x => x.GymName)
        //                 .ToListAsync(ct);

        //             return gyms
        // .GroupBy(x => x.GymId)
        // .Select(g => g.First())
        // .ToList();

        //         }




        public async Task<UserGymsListRDTO> GetUserGymsAsync(int userId, UserGymsPagedReq req, CancellationToken cancellationToken)
        {
            var ownerGymsQuery = context.Gym
                .Where(x => x.CreatedById == userId && x.IsActive);

            var staffGymsQuery = context.GymStaff
                .Include(x => x.Gym)
                .Where(x => x.UserId == userId && x.IsActive)
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
                    var staff = await context.GymStaff
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.GymId == gym.Id && x.IsActive, cancellationToken);
                    if (staff != null)
                    {
                        role = staff.GymRole.ToString();
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

            if (!string.IsNullOrEmpty(req.SortBy))
            {
                if (req.SortBy.Equals("GymName", StringComparison.OrdinalIgnoreCase))
                {
                    resultList = req.IsAscending
                        ? resultList.OrderBy(x => x.GymName).ToList()
                        : resultList.OrderByDescending(x => x.GymName).ToList();
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