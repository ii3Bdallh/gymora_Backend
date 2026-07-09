// Application/Service/Base/BaseAuditableService.cs
using Application.DTO;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using AutoMapper;
using Domain.Model.Base;
using MassTransit;
using Application.Cache;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.DTO.Base.Auditable;
using Domain.Events;

namespace Application.Service.Base
{
    /// <summary>
    /// Base service for entities that are auditable and visible to users within the same gym.
    /// مثال: Gym, Member, WorkoutPlan, Exercise, Revenue, etc.
    /// </summary>
    public abstract class BaseAuditableService<T, RDTO, CDTO, UDTO>
        : BaseService<T, RDTO, CDTO, UDTO>
        where T : AuditableEntity
        where RDTO : BaseAuditableRDTO
        where CDTO : BaseAuditableCDTO
        where UDTO : BaseAuditableUDTO
    {
        protected BaseAuditableService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser)
        {
        }

        // Read Operations: متاحة للكل داخل الجيم
        public override async Task<RDTO> GetByIdAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken ct = default)
        {
            return await base.GetByIdAsync(id, isActive, trackChanges, ct);
        }

        public override async Task<PaginatedRes<RDTO>> GetPageAsync(PaginatedSearchReq searchReq, bool isActive = true, bool trackChanges = false, CancellationToken ct = default)
        {
            return await base.GetPageAsync(searchReq, isActive, trackChanges, ct);
        }

        public override async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {

            dto.CreatedById = CurrentUserId;

            return await base.AddAsync(dto, cancellationToken);
        }

        public override async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {

            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);

            if (entity == null || !CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            dto.CreatedById = entity.CreatedById; // Preserve the original CreatedById

            _mapper.Map(dto, entity);

            T updated = await _repo.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(updated);

        }

        public override async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);

            if (entity == null || !CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            T deleted = await _repo.DeleteAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(deleted);
        }

        protected virtual bool CanModify(T entity)
        {
            if (CurrentUser.IsSuperAdmin)
                return true;

            // Owner or Admin in the gym
            // if (CurrentUser.IsInGymRole("Owner") || CurrentUser.IsInGymRole("Admin"))
            //     return true;

            // Owner of the record
            if (entity.CreatedById == CurrentUserId)
                return true;

            return false;
        }
    }
}