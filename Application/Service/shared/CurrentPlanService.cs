using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Domain.Enum;

namespace Application.Service.shared
{
    public class CurrentPlanService : ICurrentPlanService
    {
        private readonly IOwnerSubscriptionRepo _subscriptionRepo;
        private readonly ISubscriptionPlanRepo _planRepo;

        public CurrentPlanService(
            IOwnerSubscriptionRepo subscriptionRepo,
            ISubscriptionPlanRepo planRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _planRepo = planRepo;
        }

        public Task CheckCoachLimitAsync(int gymId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CheckGymLimitAsync(int ownerUserId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CheckMemberLimitAsync(int gymId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<CurrentPlanResult> GetCurrentPlanAsync(
      int ownerUserId,
      CancellationToken ct = default)
        {
            var subscription =
                await _subscriptionRepo.GetCurrentSubscriptionAsync(ownerUserId, ct);

            if (subscription != null &&
                (subscription.Status == OwnerSubscriptionStatus.Active ||
                 subscription.Status == OwnerSubscriptionStatus.Grace))
            {
                return new CurrentPlanResult
                {
                    PlanId = subscription.Plan.Id,
                    PlanName = subscription.Plan.Name,

                    MaxOwnedGyms = subscription.Plan.MaxOwnedGyms,
                    MaxMembersPerGym = subscription.Plan.MaxMembersPerGym,
                    MaxCoachesPerGym = subscription.Plan.MaxCoachesPerGym,

                    Subscription = subscription,
                    SubscriptionStatus = subscription.Status,
                    IsFree = false
                };
            }

            var freePlan = await _planRepo.GetFreePlanAsync(ct);

            if (freePlan == null)
                throw new ApplicationException("Free subscription plan was not found.");

            return new CurrentPlanResult
            {
                PlanId = freePlan.Id,
                PlanName = freePlan.Name,

                MaxOwnedGyms = freePlan.MaxOwnedGyms,
                MaxMembersPerGym = freePlan.MaxMembersPerGym,
                MaxCoachesPerGym = freePlan.MaxCoachesPerGym,

                IsFree = true,
                Subscription = null,
                SubscriptionStatus = null
            };
        }
    }
}