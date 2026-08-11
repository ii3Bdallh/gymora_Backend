using Domain.Model;
using Application.Interface.Repo;

namespace Application.Interface.Repo
{
    public interface IInvitationRepo : IBaseRepo<Invitation>
    {
        Task<bool> HasPendingInvitationAsync(int gymId, int userId, System.Threading.CancellationToken ct = default);
    }
}
