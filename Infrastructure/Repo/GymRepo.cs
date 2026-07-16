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
    public class GymRepo(ApplicationDbContext context, ILogger<GymRepo> logger, QueryCache queryCache, CurrentUser currentUser)
    : BaseRepo<Gym>(context, logger, queryCache, currentUser), IGymRepo
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
    }
}