using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class ExerciseRepo(ApplicationDbContext context, ILogger<ExerciseRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseRepo<Exercise>(context, logger, queryCache), IExerciseRepo
    {
        private readonly CurrentUser _currentUser = currentUser;

        protected override IQueryable<Exercise> ApplyExtraFilters(IQueryable<Exercise> query)
        {
            query = base.ApplyExtraFilters(query);

            if (_currentUser.IsSuperAdmin)
                return query;

            // Non-SuperAdmins can only see approved exercises or exercises they created themselves
            return query.Where(x => x.IsApproved || x.CreatedById == _currentUser.UserId);
        }
    }
}
