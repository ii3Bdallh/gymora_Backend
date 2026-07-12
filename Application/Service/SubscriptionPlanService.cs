using Application.Cache;
using Application.DTO;
using Application.DTO.CRUD.Create;
using Application.DTO.CRUD.Read;
using Application.DTO.CRUD.Update;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service.Entity;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Service.Entity
{
    public class SubscriptionPlanService : BaseService<SubscriptionPlan, SubscriptionPlanRDTO, SubscriptionPlanCDTO, SubscriptionPlanUDTO>, ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepo _subscriptionPlanRepo;
        public SubscriptionPlanService(
            ISubscriptionPlanRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<SubscriptionPlanService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _subscriptionPlanRepo = repo;
        }

        public async Task<PlanPrice> AddPlanPriceAsync(PlanPrice planPrice, int PlanId, CancellationToken cancellationToken = default)
        {
            SubscriptionPlan? entity = await _subscriptionPlanRepo.GetByIdAsync(PlanId, isActive: true, trackChanges: false, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(SubscriptionPlan).Name} with ID {PlanId} was not found.");

            await _subscriptionPlanRepo.AddPlanPriceAsync(planPrice, cancellationToken);
            return planPrice;
        }

        public async Task<PlanPrice> DeletePlanPriceAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            await _subscriptionPlanRepo.DeletePlanPriceAsync(entity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PlanPrice>(entity);
        }

        public async Task<PlanPrice?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, isActive: true, trackChanges: false, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            return _mapper.Map<PlanPrice>(entity);
        }
    }
}
