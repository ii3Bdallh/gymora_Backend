using Domain.Interface;
using Domain.Model.Base;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Model;

namespace Infrastructure.Repo.Base
{
    public abstract class BaseAuditableGymRepo<T> : BaseGymRepo<T>
        where T : class,IBaseEntity,IBaseGymEntity , IBaseAuditableGymEntity
    {

        protected BaseAuditableGymRepo(ApplicationDbContext context, ILogger<BaseRepo<T>> logger, QueryCache queryCache, CurrentUser currentUser)
            : base(context, logger, queryCache, currentUser)
        {
        }

        protected override IQueryable<T> ApplyExtraFilters(IQueryable<T> query)
        {
            query = base.ApplyExtraFilters(query);



            if (!currentUser.CurrentStaffId.HasValue)
                return query.Where(_ => false);

            return query.Where(x =>
                EF.Property<int>(x, nameof(IBaseAuditableGymEntity.CreatedByStaffId)) == currentUser.CurrentStaffId.Value);
        }
    }
}