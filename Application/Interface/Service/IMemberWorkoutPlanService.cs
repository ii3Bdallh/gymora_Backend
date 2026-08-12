using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface IMemberWorkoutPlanService : IBaseService<MemberWorkoutPlan, MemberWorkoutPlanRDTO, MemberWorkoutPlanCDTO, MemberWorkoutPlanUDTO>
    {
        Task CancelAssignmentAsync(int memberWorkoutPlanId, CancellationToken ct);
    }
}
