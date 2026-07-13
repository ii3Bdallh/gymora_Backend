using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Base;
using Application.DTO.Pagintion;
using Domain.Model.Base;

namespace Application.Interface.Service
{
    /// <summary>
    /// Read-only contract. Any service that only needs to expose reads
    /// should implement this instead of the full IBaseService.
    /// </summary>
    public interface IBaseReadService<T, RDTO>
        where T : BaseEntity
        where RDTO : BaseRDTO
    {
        Task<IEnumerable<RDTO>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<RDTO> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        Task<PaginatedRes<RDTO>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default);


    }
}
