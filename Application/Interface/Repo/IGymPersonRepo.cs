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

        Task<int> CountPeopleTypeByOwnerAsync(int ownerUserId, PersonType personType, CancellationToken ct = default);

    }
}
