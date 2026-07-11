using Application.Cache;
using Application.DTO.Base.Auditable;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Events;
using Domain.Model.Base;
using MassTransit;

namespace Application.Service.Base
{
    public abstract class BaseAuditableFileService<T, RDTO, CDTO, UDTO>
        : BaseAuditableService<T, RDTO, CDTO, UDTO>
        where T : BaseAuditableFileEntity
        where RDTO : BaseAuditableFRDTO
        where CDTO : BaseAuditableFCDTO
        where UDTO : BaseAuditableFUDTO
    {
        protected readonly IBunnyStorageService _bunnyStorageService;

        protected BaseAuditableFileService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            IBunnyStorageService bunnyStorageService)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser)
        {
            _bunnyStorageService = bunnyStorageService;
        }

        public override async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            dto.CreatedById = CurrentUserId;

            string fileUrl = await _bunnyStorageService.UploadFileToBunnyStorageAsync(dto.File, cancellationToken);

            T entity = _mapper.Map<T>(dto);
            entity.FileUrl = fileUrl;
            entity.StoredFileName = dto.File.FileName;

            T added = await _repo.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), added.Id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(added);
        }

        public override async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);

            if (entity is null || !CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            dto.CreatedById = entity.CreatedById;

            if (dto.File is not null)
            {
                string oldStoredFileName = entity.StoredFileName;

                string newFileUrl = await _bunnyStorageService.UploadFileToBunnyStorageAsync(dto.File, cancellationToken);

                _mapper.Map(dto, entity);
                entity.FileUrl = newFileUrl;
                entity.StoredFileName = dto.File.FileName;

                if (!string.IsNullOrWhiteSpace(oldStoredFileName))
                    await _bunnyStorageService.DeleteFileFromBunnyStorageAsync(oldStoredFileName, cancellationToken);
            }
            else
            {
                _mapper.Map(dto, entity);
            }

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

            if (entity is null || !CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            T deleted = await _repo.DeleteAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(deleted.StoredFileName))
                await _bunnyStorageService.DeleteFileFromBunnyStorageAsync(deleted.StoredFileName, cancellationToken);

            return _mapper.Map<RDTO>(deleted);
        }
    }
}
