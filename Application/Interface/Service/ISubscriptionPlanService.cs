
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
        public Task<PlanPriceRDTO> AddPlanPriceAsync(int PlanId, PlanPriceCDTO dto, CancellationToken cancellationToken = default);

        public Task<PlanPriceRDTO> DeletePlanPriceAsync(int id, CancellationToken cancellationToken = default);

        public Task<PlanPriceRDTO?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default);

        public Task<PlanPriceRDTO> UpdatePlanPriceAsync(int id, PlanPriceUDTO dto, CancellationToken cancellationToken = default);


    }
}

