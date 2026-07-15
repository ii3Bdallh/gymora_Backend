using Application.Cache;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Events;
using Domain.Model.Base;
using MassTransit;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Application.Service.Base
{
    /// <summary>
    /// Base service for auditable entities containing files
    /// </summary>
    public abstract class BaseAuditableFileService<T, RDTO, CDTO, UDTO>
        : BaseAuditableService<T, RDTO, CDTO, UDTO>
        where T : BaseAuditableFileEntity
        where RDTO : BaseAuditableFRDTO
        where CDTO : BaseAuditableFCDTO
        where UDTO : BaseAuditableFUDTO
    {
        protected readonly IStorageService _storageService;

        protected BaseAuditableFileService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            IStorageService storageService,
            ILogger logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _storageService = storageService;
        }

        public override async Task<RDTO> GetByIdAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken ct = default)
        {
            var entityName = CacheEntityNames.ForType<T>();
            var key = CacheKeyGenerator.ById(entityName, id, CurrentGymId);

            var cached = await _cacheService.GetAsync<RDTO>(key);
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit for auditable {EntityType} file entity with ID {Id}", typeof(T).Name, id);
                return cached;
            }

            _logger.LogInformation("Fetching auditable {EntityType} file entity with ID {Id}", typeof(T).Name, id);
            T? entity = await _repo.GetByIdAsync(id, isActive, trackChanges, ct);

            if (entity is null)
            {
                _logger.LogWarning("Auditable {EntityType} file entity with ID {Id} was not found", typeof(T).Name, id);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            var dto = _mapper.Map<RDTO>(entity);
            await _cacheService.SetAsync(key, dto);
            if (!string.IsNullOrEmpty(entity.StoredFileName))
            {
                dto.FileUrl = _storageService.GetFileAccessUrl(
                    entity.StoredFileName,
                    entity.IsPublic);
            }

            return dto;
        }

        public override async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            string storedPath = string.Empty;
            dto.CreatedById = CurrentUserId;
            _logger.LogInformation("Uploading file for auditable {EntityType} by user {UserId}", typeof(T).Name, CurrentUserId);

            if (dto.File != null)
                storedPath = await _storageService.UploadFileToStorageAsync(
                   dto.File,
                   dto.IsPublic,
                   typeof(T).Name.Replace("Entity", ""),
                   cancellationToken);

            T entity = _mapper.Map<T>(dto);
            entity.StoredFileName = storedPath;
            entity.IsPublic = dto.IsPublic;

            if (dto.IsPublic)
                entity.FileUrl = _storageService.GetFileAccessUrl(storedPath, true);

            T added = await _repo.AddAsync(entity, cancellationToken);


            await PublishEntityChangedAsync(added.Id, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auditable {EntityType} file entity with ID {Id} added successfully (Public: {IsPublic})", typeof(T).Name, added.Id, dto.IsPublic);
            return _mapper.Map<RDTO>(added);
        }

        public override async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            dto.CreatedById = CurrentUserId;

            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);
            if (entity == null || !CanModify(entity))
            {
                _logger.LogWarning("Unauthorized or failed attempt to update auditable {EntityType} file entity with ID {Id} by user {UserId}", typeof(T).Name, id, CurrentUserId);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            if (dto.File is not null)
            {
                _logger.LogInformation("Replacing file for auditable {EntityType} with ID {Id}", typeof(T).Name, id);
                string oldStoredPath = entity.StoredFileName;
                bool currentIsPublicStatus = entity.IsPublic;

                string newStoredPath = await _storageService.UploadFileToStorageAsync(
                    dto.File,
                    currentIsPublicStatus,
                    typeof(T).Name.Replace("Entity", ""),
                    cancellationToken);

                _mapper.Map(dto, entity);

                entity.StoredFileName = newStoredPath;
                entity.IsPublic = currentIsPublicStatus;

                if (currentIsPublicStatus)
                    entity.FileUrl = _storageService.GetFileAccessUrl(newStoredPath, true);
                else
                    entity.FileUrl = null;

                if (!string.IsNullOrWhiteSpace(oldStoredPath))
                {
                    await _storageService.DeleteFileFromStorageAsync(oldStoredPath, cancellationToken);
                    _logger.LogInformation("Old file deleted for auditable {EntityType} with ID {Id}", typeof(T).Name, id);
                }
            }
            else
            {
                _mapper.Map(dto, entity);
            }

            T updated = await _repo.UpdateAsync(entity, cancellationToken);


            _logger.LogInformation("📢 Publishing EntityChangedEvent for {Entity} ID {Id} | FileChanged: {FileChanged}",
    CacheEntityNames.ForType<T>(), id, dto.File != null);

            await PublishEntityChangedAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Auditable {EntityType} file entity with ID {Id} updated successfully", typeof(T).Name, id);
            return _mapper.Map<RDTO>(updated);
        }

        public override async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);
            if (entity == null || !CanModify(entity))
            {
                _logger.LogWarning("Unauthorized or failed attempt to delete auditable {EntityType} file entity with ID {Id} by user {UserId}", typeof(T).Name, id, CurrentUserId);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            _logger.LogInformation("Deleting auditable {EntityType} file entity with ID {Id}", typeof(T).Name, id);
            T deleted = await _repo.DeleteAsync(entity, cancellationToken);

            await PublishEntityChangedAsync(id, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);


            if (!string.IsNullOrWhiteSpace(deleted.StoredFileName))
            {
                await _storageService.DeleteFileFromStorageAsync(deleted.StoredFileName, cancellationToken);
                _logger.LogInformation("File deleted from storage for auditable {EntityType} with ID {Id}", typeof(T).Name, id);
            }

            return _mapper.Map<RDTO>(deleted);
        }
    }
}