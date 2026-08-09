using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo
{
    public class MembershipPlanRepo(ApplicationDbContext context, ILogger<MembershipPlanRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<MembershipPlan>(context, logger, queryCache, currentUser), IMembershipPlanRepo
    {
    }
}
