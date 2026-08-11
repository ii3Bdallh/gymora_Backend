using Domain.Enum;
using Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IGymPersonRepo : IBaseRepo<GymPerson>
    {
        public Task<GymPerson?> LinkAccountToGymAsync(int gymId, Guid inviteCode, CancellationToken ct = default);


        public Task<GymPerson?> GetGymOwnerAsync(int gymId, CancellationToken ct = default);
        public Task<GymPerson?> GetGymPersonAsync(int gymId, int userId , CancellationToken ct = default);
        public Task<GymPerson?> GetGymPersonByEmailAsync(int gymId, string email, CancellationToken ct = default);

        Task<int> CountPeopleTypeByOwnerAsync(int ownerUserId, PersonType personType, CancellationToken ct = default);
        Task<int> GetActiveMembersCountAsync(int gymId, CancellationToken ct = default);
        Task<int> GetExpiredMembersCountAsync(int gymId, CancellationToken ct = default);
        // Task<List<GymPerson>> GetMembersForReportAsync(int gymId, CancellationToken ct = default);
    }
}
