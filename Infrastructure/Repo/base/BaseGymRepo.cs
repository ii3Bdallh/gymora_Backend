using Domain.Interface;
using Domain.Model.Base;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Model;

namespace Infrastructure.Repo.Base
{
    public abstract class BaseGymRepo<T> : BaseRepo<T>
        where T : class, IBaseEntity, IBaseGymEntity
    {
        protected readonly CurrentUser currentUser;

        protected BaseGymRepo(ApplicationDbContext context, ILogger<BaseRepo<T>> logger, QueryCache queryCache, CurrentUser currentUser)
            : base(context, logger, queryCache)
        {
            this.currentUser = currentUser;
        }

        protected override IQueryable<T> ApplyExtraFilters(IQueryable<T> query)
        {
            query = base.ApplyExtraFilters(query);
            // SuperAdmin can see all gyms
            if (currentUser.IsSuperAdmin)
                return query;

            // User Dont Have GymId Can't See Any Gym
            if (!currentUser.CurrentGymId.HasValue)
                return query.Where(_ => false);

            // User Can See Only His Gym
            return query.Where(x => x.GymId == currentUser.CurrentGymId.Value);
        }


    }
}