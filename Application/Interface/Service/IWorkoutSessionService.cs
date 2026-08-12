using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface ISessionService : IBaseService<Session, SessionRDTO, SessionCDTO, SessionUDTO>
    {
        Task<SessionExerciseRDTO> AddExerciseToSessionAsync(int sessionId, SessionExerciseCDTO dto, CancellationToken ct);
        Task RemoveExerciseFromSessionAsync(int sessionId, int exerciseId, CancellationToken ct);
        Task ApproveAsync(int id, CancellationToken cancellationToken);
    }
}
