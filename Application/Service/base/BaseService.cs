using Application.Cache;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Events;
using Domain.Model.Base;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    /// <summary>
    /// Adds Add/Update/Delete on top of BaseReadService. Because it
    /// inherits from BaseReadService, it automatically has GetAll/GetById/
    /// GetPage too — a write-capable service is always read-capable first.
    /// </summary>
    public abstract class BaseService<T, RDTO, CDTO, UDTO>
        : BaseReadService<T, RDTO>, IBaseService<T, RDTO, CDTO, UDTO>
        where T : BaseEntity
        where RDTO : BaseRDTO
        where CDTO : BaseCDTO
        where UDTO : BaseUDTO
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IPublishEndpoint _publishEndpoint;

        protected BaseService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger logger)
            : base(repo, mapper, cacheService, currentUser, logger)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
        }
        #region Add
        public virtual async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            await BeforeAddAsync(dto, cancellationToken);

            var entity = _mapper.Map<T>(dto);

            await AfterMapAddAsync(entity, dto, cancellationToken);

            entity = await _repo.AddAsync(entity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await PublishEntityChangedAsync(entity.Id, cancellationToken);

            return _mapper.Map<RDTO>(entity);
        }

        protected virtual Task BeforeAddAsync(
    CDTO dto,
    CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterMapAddAsync(
    T entity,
    CDTO dto,
    CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region Update



        public virtual async Task<RDTO> UpdateAsync(
            int id,
            UDTO dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await LoadForUpdateAsync(id, cancellationToken);

            await BeforeUpdateAsync(entity, dto, cancellationToken);

            _mapper.Map(dto, entity);

            await AfterMapUpdateAsync(entity, dto, cancellationToken);

            entity = await _repo.UpdateAsync(entity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await PublishEntityChangedAsync(entity.Id, cancellationToken);

            await AfterUpdateAsync(entity, cancellationToken);

            return _mapper.Map<RDTO>(entity);
        }
        protected virtual async Task<T> LoadForUpdateAsync(
    int id,
    CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(
                id,
                isActive: true,
                trackChanges: true,
                cancellationToken);

            if (entity is null)
            {
                _logger.LogWarning("{EntityType} with ID {Id} was not found for update", typeof(T).Name, id);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            return entity;
        }

        protected virtual Task BeforeUpdateAsync(
    T entity,
    UDTO dto,
    CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        protected virtual Task AfterMapUpdateAsync(
            T entity,
            UDTO dto,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterUpdateAsync(
    T entity,
    CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region Delete     
        public virtual async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await LoadForUpdateAsync(id, cancellationToken);

            await BeforeDeleteAsync(entity, cancellationToken);

            await _repo.DeleteAsync(entity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await PublishEntityChangedAsync(entity.Id, cancellationToken);


            await AfterDeleteAsync(entity, cancellationToken);

            return _mapper.Map<RDTO>(entity);
        }

        protected virtual Task BeforeDeleteAsync(
    T entity,
    CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterDeleteAsync(
    T entity,
    CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion
        protected virtual Task PublishEntityChangedAsync(
    int entityId,
    CancellationToken cancellationToken = default)
        {
            return _publishEndpoint.Publish(
                new EntityChangedEvent(
                    CacheEntityNames.ForType<T>(),
                    entityId,
                    CurrentGymId,
                    CurrentUserId),
                cancellationToken);
        }
    }
}