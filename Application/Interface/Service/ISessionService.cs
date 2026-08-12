using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface ISessionService : IBaseService<Session, SessionRDTO, SessionCDTO, SessionUDTO>
    {
        Task ApproveAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<SessionRDTO>> AddRangeAsync(IEnumerable<SessionCDTO> dtos, CancellationToken cancellationToken);
    }
}
