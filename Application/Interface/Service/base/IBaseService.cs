using Application.DTO;
using Application.DTO.Pagintion;

namespace Application.Interface.Service
{
    public interface IBaseService<T, RDTO, CDTO, UDTO>
    {
        Task<IEnumerable<RDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PaginatedRes<RDTO>> GetPageAsync(PaginatedSearchReq searchReq, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default);
        Task<RDTO> GetByIdAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default);
        Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default);
        Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default);
        Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}