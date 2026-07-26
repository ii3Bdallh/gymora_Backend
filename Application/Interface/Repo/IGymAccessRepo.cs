using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Model;

namespace Application.Interface.Repo
{
    public interface IGymAccessRepo
    {
        Task<List<MyGymDto>> GetMyGymsAsync(
    int userId,
    CancellationToken ct = default);

        Task<MyGymDto?> GetGymAccessAsync(
            int userId,
            int gymId,
            CancellationToken ct = default);

        Task<LoginResDto> SwitchGymAsync(
            SwitchGymRequest request,
            CancellationToken ct);
    }
}