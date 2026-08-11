using Application.DTO.Model;
using Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IInvitationService : IBaseService<Invitation, InvitationRDTO, InvitationCDTO, InvitationUDTO>
    {
        Task<InvitationRDTO> CreateInvitationAsync(InvitationCDTO dto, CancellationToken ct = default);
        Task<InvitationRDTO> AcceptInvitationAsync(int invitationId, CancellationToken ct = default);
        Task<InvitationRDTO> RejectInvitationAsync(int invitationId, CancellationToken ct = default);
        Task<InvitationRDTO> CancelInvitationAsync(int invitationId, CancellationToken ct = default);
    }
}
