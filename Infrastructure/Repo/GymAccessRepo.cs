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
            var staffGym = await context.GymPerson
                .AsNoTracking()
                .Include(x => x.Gym)
                .Include(x => x.StaffProfile)
                .Where(x =>
                    x.UserId == userId &&
                    x.GymId == gymId &&
                    x.IsActive &&
                    (x.PersonType == PersonType.Staff || x.PersonType == PersonType.Both))
                .Select(x => new MyGymDto
                {
                    GymPeopleId = x.Id,
                    GymId = x.GymId,
                    GymName = x.Gym.Name,
                    GymRole = x.StaffProfile != null ? x.StaffProfile.GymRoleId.ToString() : GymRole.Other.ToString(),
                })
                .FirstOrDefaultAsync(ct);

            if (staffGym is not null)
                return staffGym;


            return null;
        }

  






    }
}