
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO.CRUD.Read;
using Application.DTO.CRUD.Create;
using Application.DTO.CRUD.Update;
using Application.DTO;
using Application.DTO.Pagintion;


namespace Application.Interface.Service.Entity
{
    public interface ISubscriptionPlanService : IBaseService<SubscriptionPlan, SubscriptionPlanRDTO, SubscriptionPlanCDTO, SubscriptionPlanUDTO>
    {
        public Task<PlanPrice> AddPlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default);

        public Task<PlanPrice> DeletePlanPriceAsync(int id, CancellationToken cancellationToken = default);

        public Task<PlanPrice?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default);


    }
}

