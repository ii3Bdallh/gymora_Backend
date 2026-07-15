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


        protected virtual async Task ReplaceFileAsync(
            T entity,
            BaseFUDTO dto,
            CancellationToken cancellationToken)
        {
            var oldStoredFile = entity.StoredFileName;

            var newStoredFile = await _storageService.UploadFileToStorageAsync(
                dto.File!,
                entity.IsPublic,
                typeof(T).Name.Replace("Entity", ""),
                cancellationToken);

            _mapper.Map(dto, entity);

            entity.StoredFileName = newStoredFile;

            entity.FileUrl = entity.IsPublic
                ? _storageService.GetFileAccessUrl(newStoredFile, true)
                : null;

            if (!string.IsNullOrWhiteSpace(oldStoredFile))
            {
                await _storageService.DeleteFileFromStorageAsync(
                    oldStoredFile,
                    cancellationToken);
            }
        }


        #region Read

        protected override Task AfterMapReadAsync(
        T entity,
        RDTO dto,
        CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(entity.StoredFileName))
            {
                dto.FileUrl = _storageService.GetFileAccessUrl(
                    entity.StoredFileName,
                    entity.IsPublic);
            }

            return Task.CompletedTask;
        }

        #endregion


        protected virtual Task BeforeAddFileAsync(
            CDTO dto,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        protected virtual Task BeforeUpdateFileAsync(
            T entity,
            UDTO dto,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task BeforeDeleteFileAsync(
            T entity,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #region Add

        protected override async Task BeforeAddAsync(
    CDTO dto,
    CancellationToken cancellationToken)
        {
            // اعملها لو في الاب حاجه عاوز اعملها االاول 
            // await base.BeforeAddAsync(dto, cancellationToken); 

            await BeforeAddFileAsync(dto, cancellationToken);
        }

        #endregion

        #region Update

        protected override async Task AfterMapUpdateAsync(
    T entity,
    UDTO dto,
    CancellationToken cancellationToken)
        {
            // اعملها لو في الاب حاجه عاوز اعملها االاول 
            // await base.AfterMapUpdateAsync(entity, dto, cancellationToken);

            await BeforeUpdateFileAsync(entity, dto, cancellationToken);

            if (dto.File is not null)
            {
                await ReplaceFileAsync(entity, dto, cancellationToken);
            }
        }
        #endregion

        #region Delete

        protected override async Task BeforeDeleteAsync(
    T entity,
    CancellationToken cancellationToken)
        {
            // اعملها لو في الاب حاجه عاوز اعملها االاول 
            // await base.BeforeDeleteAsync(entity, cancellationToken);

            await BeforeDeleteFileAsync(entity, cancellationToken);
        }

        protected override async Task AfterDeleteAsync(
    T entity,
    CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(entity.StoredFileName))
            {
                await _storageService.DeleteFileFromStorageAsync(
                    entity.StoredFileName,
                    cancellationToken);
            }
            // اعملها لو في الاب حاجه عاوز اعملها االاول 
            // await base.AfterDeleteAsync(entity, cancellationToken);
        }
        #endregion
    }
}