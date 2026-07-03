using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Model.Base;

namespace Application.Interface.Repo.Base
{
    public interface IOwnershipRepo<T> : IBaseRepo<T>
        where T : AuditableEntity
    {
        Task<IEnumerable<T?>> GetAllOwnedAsync(
            CancellationToken cancellationToken = default
     );

        IQueryable<T> OwnedQuery(
            PaginatedSearchReq searchReq,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        Task<PaginatedRes<T>> GetPageOwnedAsync(
            PaginatedSearchReq searchReq,
            bool IsActive = true,
            bool trackChanges = false, CancellationToken cancellationToken = default);

        Task<T?> GetByIdOwnedAsync(
            int id,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default);
    }

}