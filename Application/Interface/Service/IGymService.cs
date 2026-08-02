using Application.DTO;
using Application.DTO.Model;
using Domain.Model;
using Gymora.Contracts.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface IGymService : IBaseService<Gym, GymRDTO, GymCDTO, GymUDTO>
    {

        Task ChangeOwnerOfGymAsync(int gymId, int newOwnerUserId, CancellationToken ct = default);

        Task<UserGymsListRDTO> GetUserGymsAsync( UserGymsPagedReq req, CancellationToken cancellationToken);


    }
}