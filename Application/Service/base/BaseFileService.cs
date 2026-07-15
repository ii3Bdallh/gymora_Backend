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
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    /// <summary>
    /// Base service for entities containing files (non-auditable version)
    /// </summary>
    public abstract class BaseFileService<T, RDTO, CDTO, UDTO>
        : BaseService<T, RDTO, CDTO, UDTO>
        where T : BaseFileEntity
        where RDTO : BaseFRDTO
        where CDTO : BaseFCDTO
        where UDTO : BaseFUDTO
    {
        protected readonly IStorageService _storageService;

        protected BaseFileService(
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
                _logger.LogInformation("Cache hit for {EntityType} file entity with ID {Id}", typeof(T).Name, id);
                return cached;
            }

            _logger.LogInformation("Fetching {EntityType} file entity with ID {Id}", typeof(T).Name, id);
            T? entity = await _repo.GetByIdAsync(id, isActive, trackChanges, ct);

            if (entity is null)
            {
                _logger.LogWarning("{EntityType} file entity with ID {Id} was not found", typeof(T).Name, id);
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
            _logger.LogInformation("Uploading file for {EntityType}", typeof(T).Name);
            string storedPath = string.Empty;

            // رفع الملف مع التحكم في نوعه
            if (dto.File != null)
                storedPath = await _storageService.UploadFileToStorageAsync(
                   dto.File,
                   dto.IsPublic,
                   typeof(T).Name.Replace("Entity", ""),
                   cancellationToken);


            T entity = _mapper.Map<T>(dto);
            entity.StoredFileName = storedPath;
            entity.IsPublic = dto.IsPublic;

            // نحفظ FileUrl فقط إذا كان Public
            if (dto.IsPublic)
                entity.FileUrl = _storageService.GetFileAccessUrl(storedPath, true);

            T added = await _repo.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await PublishEntityChangedAsync(added.Id, cancellationToken);

            _logger.LogInformation("{EntityType} file entity with ID {Id} added successfully (Public: {IsPublic})", typeof(T).Name, added.Id, dto.IsPublic);
            return _mapper.Map<RDTO>(added);
        }

        public override async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);
            if (entity is null)
            {
                _logger.LogWarning("{EntityType} file entity with ID {Id} was not found for update", typeof(T).Name, id);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            if (dto.File is not null)
            {
                _logger.LogInformation("Replacing file for {EntityType} with ID {Id}", typeof(T).Name, id);
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
                    entity.FileUrl = null; // مهم: نمسح الرابط القديم

                // حذف الملف القديم
                if (!string.IsNullOrWhiteSpace(oldStoredPath))
                {
                    await _storageService.DeleteFileFromStorageAsync(oldStoredPath, cancellationToken);
                    _logger.LogInformation("Old file deleted for {EntityType} with ID {Id}", typeof(T).Name, id);
                }
            }
            else
            {
                _mapper.Map(dto, entity);
            }

            T updated = await _repo.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await PublishEntityChangedAsync(id, cancellationToken);

            _logger.LogInformation("{EntityType} file entity with ID {Id} updated successfully", typeof(T).Name, id);
            return _mapper.Map<RDTO>(updated);
        }

        public override async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);
            if (entity is null)
            {
                _logger.LogWarning("{EntityType} file entity with ID {Id} was not found for deletion", typeof(T).Name, id);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            _logger.LogInformation("Deleting {EntityType} file entity with ID {Id}", typeof(T).Name, id);
            await _repo.DeleteAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await PublishEntityChangedAsync(id, cancellationToken);

            if (!string.IsNullOrWhiteSpace(entity.StoredFileName))
            {
                await _storageService.DeleteFileFromStorageAsync(entity.StoredFileName, cancellationToken);
                _logger.LogInformation("File deleted from storage for {EntityType} with ID {Id}", typeof(T).Name, id);
            }

            return _mapper.Map<RDTO>(entity);
        }
    }
}