using Application.Cache;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Model.Base;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Base
{
    /// <summary>
    /// Base service for gym-owned entities containing files (non-auditable version)
    /// </summary>
    public abstract class BaseGymFileService<T, RDTO, CDTO, UDTO>
        : BaseGymService<T, RDTO, CDTO, UDTO>
        where T : BaseGymFileEntity
        where RDTO : BaseGymFRDTO
        where CDTO : BaseGymFCDTO
        where UDTO : BaseGymFUDTO
    {
        protected readonly IStorageService _storageService;

        protected BaseGymFileService(
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

        protected virtual async Task UploadFileAndSaveFileInfoAsync(
            T entity,
            IFormFile file,
            bool isPublic,
            CancellationToken ct)
        {
            entity.StoredFilePath = await _storageService.UploadFileToStorageAsync(
                file,
                isPublic,
                typeof(T).Name.Replace("Entity", ""),
                ct);

            entity.IsPublic = isPublic;

            entity.FileUrl = isPublic
                ? _storageService.GetFileAccessUrl(entity.StoredFilePath, true)
                : null;
        }

        #region Read

        protected override Task AfterMapReadAsync(
            T entity,
            RDTO dto,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(entity.StoredFilePath))
            {
                dto.FileUrl = _storageService.GetFileAccessUrl(
                    entity.StoredFilePath,
                    entity.IsPublic);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Add

        protected override async Task AfterMapAddAsync(
            T entity,
            CDTO dto,
            CancellationToken cancellationToken)
        {
            if (dto.File is not null)
            {
                await UploadFileAndSaveFileInfoAsync(entity, dto.File, dto.IsPublic, cancellationToken);
            }
        }

        #endregion

        #region Update

        protected override async Task AfterMapUpdateAsync(
            T entity,
            UDTO dto,
            CancellationToken cancellationToken)
        {
            if (dto.File is not null)
            {
                string? oldStoredFilePath = entity.StoredFilePath;
                await UploadFileAndSaveFileInfoAsync(entity, dto.File, entity.IsPublic, cancellationToken);

                if (!string.IsNullOrWhiteSpace(oldStoredFilePath))
                {
                    await _storageService.DeleteFileFromStorageAsync(oldStoredFilePath, cancellationToken);
                    _logger.LogInformation("Old file deleted for {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
                }
            }
        }

        #endregion

        #region Delete

        protected override async Task AfterDeleteAsync(
            T entity,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(entity.StoredFilePath))
            {
                await _storageService.DeleteFileFromStorageAsync(
                    entity.StoredFilePath,
                    cancellationToken);
            }
        }

        #endregion
    }
}
