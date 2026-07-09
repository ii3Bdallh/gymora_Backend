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
            CurrentUser currentUser)
            : base(repo, mapper, cacheService, currentUser)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
        }

        public virtual async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            T entity = _mapper.Map<T>(dto);

            T added = await _repo.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), added.Id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(added);
        }

        public virtual async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            _mapper.Map(dto, entity);

            T updated = await _repo.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(updated);
        }

        public virtual async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            await _repo.DeleteAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(entity);
        }
    }
}