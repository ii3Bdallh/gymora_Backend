using Application.DTO;
using Application.DTO.Model;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface IGymService : IBaseService<Gym, GymRDTO, GymCDTO, GymUDTO>
    {

        Task<SelectGymRDTO> SwitchGymAsync(SwitchGymRequest request, CancellationToken ct = default);
        Task ChangeOwnerOfGymAsync(int gymId, int newOwnerUserId, CancellationToken ct = default);


    }
}