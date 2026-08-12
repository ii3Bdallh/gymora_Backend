using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface IWorkoutPlanService : IBaseService<WorkoutPlan, WorkoutPlanRDTO, WorkoutPlanCDTO, WorkoutPlanUDTO>
    {
        Task ApproveAsync(int id, CancellationToken cancellationToken);
    }
}
