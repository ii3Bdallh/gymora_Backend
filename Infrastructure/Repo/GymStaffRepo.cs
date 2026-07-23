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

namespace Infrastructure.Repo
{
    public class GymStaffRepo(ApplicationDbContext context, ILogger<GymStaffRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseGymRepo<GymStaff>(context, logger, queryCache, currentUser), IGymStaffRepo
    {
        public async Task<GymStaff?> LinkAccountToGymAsync(
            int gymId,
            Guid inviteCode,
            CancellationToken ct = default)
        {
            var gymStaff = await context.GymStaff.FirstOrDefaultAsync(x => x.GymId == gymId && x.StaffInviteCode == inviteCode && x.IsActive, ct);



            if (gymStaff is null)
                throw new InvalidOperationException("Invalid invite code.");

            if (gymStaff.UserId != null)
                throw new InvalidOperationException("This staff member is already linked.");

            gymStaff.UserId = currentUser.UserId;


            return gymStaff;
        }
    }
}