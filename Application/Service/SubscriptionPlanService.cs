using Application.Cache;
using Application.DTO;


using Application.DTO.Model;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Events;
using Domain.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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


        public async Task<PlanPriceRDTO> AddPlanPriceAsync(int PlanId, PlanPriceCDTO dto, CancellationToken cancellationToken = default)
        {
            SubscriptionPlan? entity = await _subscriptionPlanRepo.GetByIdAsync(PlanId, trackChanges: false, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(SubscriptionPlan).Name} with ID {PlanId} was not found.");

            PlanPrice planPrice = _mapper.Map<PlanPrice>(dto);
            planPrice.PlanId = PlanId;

            planPrice = await _subscriptionPlanRepo.AddPlanPriceAsync(planPrice, cancellationToken);

            await _publishEndpoint.Publish(
     new EntityChangedEvent(CacheEntityNames.ForType<SubscriptionPlan>(), planPrice.PlanId, CurrentGymId),
     cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<PlanPriceRDTO>(planPrice);
        }

        public async Task<PlanPriceRDTO> DeletePlanPriceAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            await _subscriptionPlanRepo.DeletePlanPriceAsync(entity, cancellationToken);


            await _publishEndpoint.Publish(
     new EntityChangedEvent(CacheEntityNames.ForType<SubscriptionPlan>(), entity.PlanId, CurrentGymId),
     cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PlanPriceRDTO>(entity);
        }

        public async Task<PlanPriceRDTO?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, trackChanges: false, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            return _mapper.Map<PlanPriceRDTO>(entity);

        }

        public async Task<PlanPriceRDTO> UpdatePlanPriceAsync(int id, PlanPriceUDTO dto, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            _logger.LogInformation("Updating PlanPrice with ID {Id}", id);
            _mapper.Map(dto, entity);
            await _publishEndpoint.Publish(
new EntityChangedEvent(CacheEntityNames.ForType<SubscriptionPlan>(), entity.PlanId, CurrentGymId),
cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("PlanPrice with ID {Id} updated successfully", id);
            return _mapper.Map<PlanPriceRDTO>(entity);
        }
    }
}
