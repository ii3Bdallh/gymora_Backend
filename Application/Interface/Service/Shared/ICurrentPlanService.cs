using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Model;

namespace Application.Interface.Service.Shared
{
    public interface ICurrentPlanService
    {
        Task<CurrentPlanResult> GetCurrentPlanAsync(
            int ownerUserId,
            CancellationToken ct = default);

        // Task<bool> HasAvailableGymSlotAsync(
        //     int ownerUserId,
        //     CancellationToken ct = default);


        // Task<bool> HasAvailableMemberSlotAsync(
        //     int ownerUserId,
        //     CancellationToken ct = default);


        // Task<bool> HasAvailableCoachSlotAsync(
        //     int ownerUserId,
        //     CancellationToken ct = default);


    }
}