using Domain.Interface;
using Domain.Model.Base;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Model;

namespace Infrastructure.Repo.Base
{

    /// <summary>
    /// Apply Filter Where CreatedBy Is Me Or SuperAdmin
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseAuditableRepo<T> : BaseRepo<T>
        where T : class, IBaseEntity, IOnlyMeCanSee
    {
        protected readonly CurrentUser currentUser;

        protected BaseAuditableRepo(ApplicationDbContext context, ILogger<BaseRepo<T>> logger, QueryCache queryCache, CurrentUser currentUser)
            : base(context, logger, queryCache)
        {
            this.currentUser = currentUser;
        }

        protected override IQueryable<T> ApplyExtraFilters(IQueryable<T> query)
        {
            query = base.ApplyExtraFilters(query);

            if (currentUser.IsSuperAdmin)
                return query;

            return query.Where(x => x.CreatedById == currentUser.UserId);
        }
    }
}