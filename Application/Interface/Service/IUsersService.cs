using Application.DTO.Model;
using Application.DTO.Pagintion;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface IUsersService
    {
        Task<IEnumerable<UsersRDTO>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<UsersRDTO> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        Task<PaginatedRes<UsersRDTO>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

    }
}