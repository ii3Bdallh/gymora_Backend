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

        Task CheckGymLimitAsync(
            int ownerUserId,
            CancellationToken ct = default);

        Task CheckMemberLimitAsync(
            int gymId,
            CancellationToken ct = default);

        Task CheckCoachLimitAsync(
            int gymId,
            CancellationToken ct = default);
    }
}