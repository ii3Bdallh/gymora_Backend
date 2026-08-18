using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class MembershipPlanRepo(ApplicationDbContext context, ILogger<MembershipPlanRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<MembershipPlan>(context, logger, queryCache, currentUser), IMembershipPlanRepo
    {
        protected override Func<IQueryable<MembershipPlan>, IQueryable<MembershipPlan>>? Includes()
            => query => query.Include(x => x.CreatedByPerson);

        public override Task<MembershipPlan?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
            => base.GetByIdAsync(id, false, cancellationToken, Includes());
    }
}
