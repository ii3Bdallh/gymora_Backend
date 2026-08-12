using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;

namespace Application.Interface.Service
{
    public interface ISessionExerciseService
    {
        Task<IEnumerable<SessionExerciseRDTO>> AddRangeAsync(IEnumerable<SessionExerciseCDTO> dtos, CancellationToken cancellationToken);
        Task DeleteRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}
