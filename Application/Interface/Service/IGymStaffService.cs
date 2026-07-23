using Application.DTO.Model;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface IGymStaffService : IBaseService<GymStaff, GymStaffRDTO, GymStaffCDTO, GymStaffUDTO>
    {

        public Task<GymStaffRDTO> GetByGymIdAndUserIdAsync(int gymId, int userId, CancellationToken ct = default);

        public Task<GymStaffRDTO> LinkAccountToGymAsync(int gymId, Guid inviteCode, CancellationToken ct = default);

        public Task PaySalaryAsync(int staffId, DateTime? salaryValidFrom, DateTime? salaryValidUntil, CancellationToken ct = default);

    }
}