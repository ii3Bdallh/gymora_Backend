using Application.DTO.Base;
using Domain.Interface;
using Domain.Model.Base;

namespace Application.Interface.Service
{
    /// <summary>
    /// Full CRUD contract. Extends IBaseReadService, so anything that
    /// can write can also read — but not the other way around.
    /// </summary>
    public interface IBaseService<T, RDTO, CDTO, UDTO> : IBaseReadService<T, RDTO>
        where T : class, IBaseEntity
        where RDTO : BaseRDTO
        where CDTO : BaseCDTO
        where UDTO : BaseUDTO
    {
        Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default);

        Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default);

        Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}