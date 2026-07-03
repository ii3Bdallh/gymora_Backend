using Application.DTO;
using Application.DTO.Base;
using Application.DTO.Errors;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using AutoMapper;
using Domain.Enum;
using Domain.Model;
using Domain.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class BaseService<T, RDTO, CDTO, UDTO> : IBaseService<T, RDTO, CDTO, UDTO>
     where T : BaseEntity
     where RDTO : BaseRDTO
     where CDTO : BaseCDTO
     where UDTO : BaseUDTO
    {
        protected readonly IBaseRepo<T> repo;
        protected readonly IMapper mapper;

        protected BaseService(IBaseRepo<T> repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }

        public virtual async Task<IEnumerable<RDTO>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var models = await repo.GetAllAsync(cancellationToken);
            return mapper.Map<IEnumerable<RDTO>>(models);
        }

        public virtual async Task<PaginatedRes<RDTO>> GetPageAsync(PaginatedSearchReq searchReq, bool IsActive = true, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            {
                var page = await repo.GetPageAsync(searchReq, IsActive, trackChanges);

                return new PaginatedRes<RDTO>
                {
                    PageNumber = page.PageNumber,
                    PageSize = page.PageSize,
                    TotalCount = page.TotalCount,
                    Items = mapper.Map<IEnumerable<RDTO>>(page.Items)
                };
            }
        }

        public virtual async Task<RDTO> GetByIdAsync(int id, bool IsActive, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            var entity = await repo.GetByIdAsync(id, IsActive, trackChanges);
            return mapper.Map<RDTO>(entity);
        }

        public virtual async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            var entity = mapper.Map<T>(dto);
            var added = await repo.AddAsync(entity, cancellationToken: cancellationToken);
            return mapper.Map<RDTO>(added);
        }

        public virtual async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            var entity = await repo.GetByIdAsync(id, true, trackChanges: true, cancellationToken: cancellationToken);

            mapper.Map(dto, entity);

            var updated = await repo.UpdateAsync(entity, cancellationToken: cancellationToken);
            return mapper.Map<RDTO>(updated);
        }

        public virtual async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var deleted = await repo.DeleteAsync(id, cancellationToken);
            return mapper.Map<RDTO>(deleted);
        }




    }

}