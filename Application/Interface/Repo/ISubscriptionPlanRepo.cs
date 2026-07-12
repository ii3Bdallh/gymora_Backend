using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface ISubscriptionPlanRepo : IBaseRepo<SubscriptionPlan>
    {

        public Task<PlanPrice> AddPlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default);

        public Task<PlanPrice> DeletePlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default);


        Task<PlanPrice?> GetPlanPriceByIdAsync(
     int id,
     bool isActive = true,
     bool trackChanges = false,
     CancellationToken cancellationToken = default);


    }
}

