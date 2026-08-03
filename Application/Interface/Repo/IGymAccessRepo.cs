using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Auth;
using Domain.Enum;
using Gymora.Contracts.Authentication;

namespace Application.Interface.Repo
{
    public interface IGymAccessRepo
    {
        // Task<List<MyGymDto>> GetMyGymsAsync(
        //     int userId,
        //     CancellationToken ct = default);

        Task<MyGymDto?> GetGymAccessAsync(
            int userId,
            int gymId,
            CancellationToken ct = default);

        // Task<IReadOnlyList<AvailableGymDto>> GetAvailableGymsAsync(int userId, CancellationToken ct = default);

        Task<bool> CanJoinGymAsync(int gymId, PersonType personType, CancellationToken ct = default);



    }
}