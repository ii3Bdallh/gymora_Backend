using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface IExerciseService : IBaseService<Exercise, ExerciseRDTO, ExerciseCDTO, ExerciseUDTO>
    {
        Task ApproveAsync(int id, CancellationToken cancellationToken);
    }
}
