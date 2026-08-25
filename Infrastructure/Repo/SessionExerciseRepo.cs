using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interface.Repo;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class SessionExerciseRepo(ApplicationDbContext context, ILogger<SessionExerciseRepo> logger, QueryCache queryCache)
        : BaseRepo<SessionExercise>(context, logger, queryCache), ISessionExerciseRepo
    {
        protected override Func<IQueryable<SessionExercise>, IQueryable<SessionExercise>>? Includes()
            => query => query.Include(x => x.Exercise);

        public override async Task<SessionExercise?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
            => await base.GetByIdAsync(id, false, cancellationToken, Includes());
    }
}
