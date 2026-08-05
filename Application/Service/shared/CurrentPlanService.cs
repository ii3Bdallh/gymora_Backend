using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using AutoMapper;
using Domain.Enum;

namespace Application.Service.shared
{
    public class CurrentPlanService : ICurrentPlanService
    {
        private readonly IOwnerSubscriptionRepo _subscriptionRepo;
        private readonly ISubscriptionPlanRepo _planRepo;
        private readonly IGymRepo _gymRepo;
        private readonly IGymPersonRepo _gymPersonRepo;
        private readonly IMapper _mapper;

        public CurrentPlanService(
            IOwnerSubscriptionRepo subscriptionRepo,
            ISubscriptionPlanRepo planRepo,
            IGymRepo gymRepo,
            IGymPersonRepo gymPersonRepo,
            IMapper mapper
            )
        {
            _subscriptionRepo = subscriptionRepo;
            _planRepo = planRepo;
            _gymRepo = gymRepo;
            _gymPersonRepo = gymPersonRepo;
            _mapper = mapper;
        }






        public async Task<CurrentPlanResult> GetCurrentPlanAsync(
      int ownerUserId,
      CancellationToken ct = default)
        {
            var subscription =
                await _subscriptionRepo.GetCurrentSubscriptionAsync(ownerUserId, ct);

            int gymCount =
 await _gymRepo.CountOwnedByOwnerAsync(ownerUserId, ct);

            int memberCount =
                await _gymPersonRepo.CountPeopleTypeByOwnerAsync(
                    ownerUserId,
                    PersonType.Member, ct);

            int coachCount =
                await _gymPersonRepo.CountPeopleTypeByOwnerAsync(
                    ownerUserId,
                    PersonType.Staff);




            if (subscription != null &&
                (subscription.Status == OwnerSubscriptionStatus.Active ||
                 subscription.Status == OwnerSubscriptionStatus.Grace))
            {


                var result = new CurrentPlanResult
                {
                    PlanId = subscription.Plan.Id,
                    PlanName = subscription.Plan.Name,

                    MaxOwnedGyms = subscription.Plan.MaxOwnedGyms,
                    MaxMembers = subscription.Plan.MaxMembers,
                    MaxCoaches = subscription.Plan.MaxCoaches,

                    IsFree = false,
                    Subscription = _mapper.Map<OwnerSubscriptionRDTO>(subscription),
                    SubscriptionStatus = subscription.Status,
                    CurrentGymCount = gymCount,
                    CurrentMemberCount = memberCount,
                    CurrentCoachCount = coachCount,
                };






                return result;
            }

            var freePlan = await _planRepo.GetFreePlanAsync(ct);

            if (freePlan == null)
                throw new ApplicationException("Free subscription plan was not found.");



            CurrentPlanResult resultFree = new CurrentPlanResult
            {
                PlanId = freePlan.Id,
                PlanName = freePlan.Name,

                MaxOwnedGyms = freePlan.MaxOwnedGyms,
                MaxMembers = freePlan.MaxMembers,
                MaxCoaches = freePlan.MaxCoaches,

                IsFree = true,
                Subscription = null,
                SubscriptionStatus = OwnerSubscriptionStatus.Active,
                CurrentGymCount = gymCount,
                CurrentMemberCount = memberCount,
                CurrentCoachCount = coachCount,
            };


            return resultFree;

        }


    }
}