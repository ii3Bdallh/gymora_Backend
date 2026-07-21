using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IGymStaffRepo : IBaseRepo<GymStaff>
    {
        public Task<GymStaff?> LinkAccountToGymAsync(int gymId,Guid inviteCode , CancellationToken ct = default);
    }
}