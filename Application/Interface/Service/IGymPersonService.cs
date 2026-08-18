using Application.DTO.Model;
using Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IGymPersonService : IBaseService<GymPerson, GymPersonRDTO, GymPersonCDTO, GymPersonUDTO>
    {

        public Task<GymPersonRDTO> LinkAccountToGymAsync(int gymId, Guid inviteCode, CancellationToken ct = default);

        public Task PaySalaryAsync(int staffId, DateTime? salaryValidFrom, DateTime? salaryValidUntil, CancellationToken ct = default);

        public Task<GymPersonRDTO> RenewMemberSubscriptionAsync(int memberId, RenewMembershipDTO dto, CancellationToken ct = default);

        public Task<GymPersonRDTO> UpdateAccessStatusAsync(int id, UpdateAccessStatusDTO dto, CancellationToken ct = default);

        public Task<GymPersonRDTO> GetMeAsync(CancellationToken ct = default);

        public Task LeaveGymAsync(int gymId, CancellationToken ct = default);
    }
}
