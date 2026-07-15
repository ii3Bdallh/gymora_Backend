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

        public virtual async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding new {EntityType}", typeof(T).Name);
            T entity = _mapper.Map<T>(dto);

            T added = await _repo.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), added.Id, CurrentGymId , CurrentUser.UserId),
                cancellationToken);

            _logger.LogInformation("{EntityType} with ID {Id} added successfully by user {UserId}", typeof(T).Name, added.Id, CurrentUserId);
            return _mapper.Map<RDTO>(added);
        }

        public virtual async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
            {
                _logger.LogWarning("{EntityType} with ID {Id} was not found for update", typeof(T).Name, id);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            _logger.LogInformation("Updating {EntityType} with ID {Id}", typeof(T).Name, id);
            _mapper.Map(dto, entity);

            T updated = await _repo.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId , CurrentUser.UserId),
                cancellationToken);

            _logger.LogInformation("{EntityType} with ID {Id} updated successfully by user {UserId}", typeof(T).Name, id, CurrentUserId);
            return _mapper.Map<RDTO>(updated);
        }

        public virtual async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
            {
                _logger.LogWarning("{EntityType} with ID {Id} was not found for deletion", typeof(T).Name, id);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            _logger.LogInformation("Deleting {EntityType} with ID {Id}", typeof(T).Name, id);
            await _repo.DeleteAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId , CurrentUser.UserId),
                cancellationToken);

            _logger.LogInformation("{EntityType} with ID {Id} deleted successfully by user {UserId}", typeof(T).Name, id, CurrentUserId);
            return _mapper.Map<RDTO>(entity);
        }
    }
}