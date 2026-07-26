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
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repo
{
    public class GymAccessRepo(ApplicationDbContext context, IAuthRepo authRepo, CurrentUser currentUser) : IGymAccessRepo
    {
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
                    GymRole = "Owner",
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
                    GymId = x.GymId,
                    GymName = x.Gym.Name,
                    GymRole = x.GymRole.ToString(),
                })
                .FirstOrDefaultAsync(ct);

            if (staffGym is not null)
                return staffGym;

            // Member
            // var memberGym = await context.GymMember
            //     .AsNoTracking()
            //     .Include(x => x.Gym)
            //     .Where(x =>
            //         x.UserId == userId &&
            //         x.GymId == gymId &&
            //         x.IsActive)
            //     .Select(x => new MyGymDto
            //     {
            //         GymId = x.GymId,
            //         GymName = x.Gym.Name,
            //         GymRole = "Member",
            //     })
            //     .FirstOrDefaultAsync(ct);


            // return memberGym;

            throw new NotImplementedException("Method not implemented");
        }

        public async Task<List<MyGymDto>> GetMyGymsAsync(
            int userId,
            CancellationToken ct = default)
        {
            var ownerGyms = context.Gym
                .AsNoTracking()
                .Where(x => x.CreatedById == userId && x.IsActive)
                .Select(x => new MyGymDto
                {
                    GymId = x.Id,
                    GymName = x.Name,
                    GymRole = "Owner"
                });

            var staffGyms = context.GymStaff
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.IsActive)
                .Select(x => new MyGymDto
                {
                    GymId = x.GymId,
                    GymName = x.Gym.Name,
                    GymRole = x.GymRole.ToString()
                });

            // لما تعمل GymMember
            /*
            var memberGyms = context.GymMember
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.IsActive)
                .Select(x => new MyGymDto
                {
                    GymId = x.GymId,
                    GymName = x.Gym.Name,
                    GymRole = "Member"
                });
            */

            var gyms = await ownerGyms
                .Union(staffGyms)
                //.Union(memberGyms)
                .OrderBy(x => x.GymName)
                .ToListAsync(ct);

            return gyms
.GroupBy(x => x.GymId)
.Select(g => g.First())
.ToList();

        }

        public async Task<LoginResDto> SwitchGymAsync(
            SwitchGymRequest request,
            CancellationToken ct)
        {
            int userId = currentUser.UserId;

            var gym = await GetGymAccessAsync(
                userId,
                request.GymId,
                ct);

            if (gym is null)
                throw new UnauthorizedException(
                    "You don't have access to this gym.");

            return await authRepo.RefreshTokenAsync(
                request.RefreshToken, ct);
        }
    }
}