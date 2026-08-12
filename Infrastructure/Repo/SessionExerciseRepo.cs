using Application.Interface.Repo;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class SessionExerciseRepo(ApplicationDbContext context, ILogger<SessionExerciseRepo> logger, QueryCache queryCache)
        : BaseRepo<SessionExercise>(context, logger, queryCache), ISessionExerciseRepo
    {
    }
}
