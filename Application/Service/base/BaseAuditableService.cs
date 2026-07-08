using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Base.Auditable;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using AutoMapper;
using Domain.Model.Base;

namespace Application.Service.Base
{
    public abstract class BaseAuditableService<T, RDTO, CDTO, UDTO> : BaseService<T, RDTO, CDTO, UDTO>
      where T : AuditableEntity
        where RDTO : BaseAuditableRDTO
        where CDTO : BaseAuditableCDTO
        where UDTO : BaseAuditableUDTO
    {
        protected readonly ICurrentUserService currentUser;
        protected BaseAuditableService(IBaseRepo<T> repo, IMapper mapper, ICurrentUserService currentUser) : base(repo,  mapper)
        {
            this.currentUser = currentUser;
        }


        public override async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            var entity = await repo.GetByIdAsync(id, true, trackChanges: true, cancellationToken: cancellationToken);
            int ownerId = entity.CreatedById;

            mapper.Map(dto, entity);
            entity.ModifiedOn = DateTime.UtcNow;
            entity.CreatedById = ownerId;
            var updated = await repo.UpdateAsync(entity, cancellationToken: cancellationToken);
            return mapper.Map<RDTO>(updated);
        }

        public override async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            dto.CreatedById = currentUser.UserId
                ?? throw new InvalidOperationException("User must be authenticated to create an entity.");
            return await base.AddAsync(dto, cancellationToken);
        }


    }
}