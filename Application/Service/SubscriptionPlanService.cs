using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service.Entity;
using Application.Service.Base;
using Application.Interface.Repo.Entity;
using Application.DTO.CRUD.Read;
using Application.DTO.CRUD.Create;
using Application.DTO.CRUD.Update;
using Application.Service.Shared;
using Application.DTO;
using Application.DTO.Pagintion;
using Application.DTO.Exceptions;

namespace Application.Service.Entity
{
    public class SubscriptionPlanService(ISubscriptionPlanRepo repo, IUnitOfWork unitOfWork, IMapper mapper)
    : BaseService<SubscriptionPlan, SubscriptionPlanRDTO, SubscriptionPlanCDTO, SubscriptionPlanUDTO>(repo, unitOfWork, mapper), ISubscriptionPlanService
    {
        public async Task<PlanPrice> AddPlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default)
        {
            SubscriptionPlan? entity = await repo.GetByIdAsync(planPrice.PlanId, isActive: true, trackChanges: false, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(SubscriptionPlan).Name} with ID {planPrice.PlanId} was not found.");

            await repo.AddPlanPriceAsync(planPrice, cancellationToken);
            return planPrice;
        }

        public async Task<PlanPrice> DeletePlanPriceAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await repo.GetPlanPriceByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);


            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            await repo.DeletePlanPriceAsync(entity, cancellationToken);


            // 👈 حفظ الـ Soft Delete
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PlanPrice>(entity);
        }

        public async Task<PlanPrice?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await repo.GetPlanPriceByIdAsync(id, isActive: true, trackChanges: false, cancellationToken: cancellationToken);

            // 👈 التحقق هنا مكانه الصحيح هندسياً
            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            return _mapper.Map<PlanPrice>(entity);
        }

    }
}


