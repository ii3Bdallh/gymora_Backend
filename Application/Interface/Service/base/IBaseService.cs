using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IBaseService<T, RDTO, CDTO, UDTO>
    {
        Task<IEnumerable<RDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PaginatedRes<RDTO>> GetPageAsync(PaginatedSearchReq searchReq, bool IsActive = true, bool trackChanges = false, CancellationToken cancellationToken = default);
        Task<RDTO> GetByIdAsync(int id, bool IsActive, bool trackChanges = false, CancellationToken cancellationToken = default);
        Task<RDTO> AddAsync(CDTO property , CancellationToken cancellationToken = default);
        Task<RDTO> UpdateAsync(int id, UDTO property, CancellationToken cancellationToken = default);
        Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default);


    }

}
