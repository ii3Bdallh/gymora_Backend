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
        : BaseFileService<T, RDTO, CDTO, UDTO>
        where T : BaseAuditableFileEntity
        where RDTO : BaseAuditableFRDTO
        where CDTO : BaseAuditableFCDTO
        where UDTO : BaseAuditableFUDTO
    {

        protected BaseAuditableFileService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            IStorageService storageService,
            ILogger logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {

        }


        protected override async Task BeforeAddAsync(
    CDTO dto,
    CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);

            dto.CreatedById = CurrentUserId;
        }

        protected override async Task BeforeUpdateAsync(
    T entity,
    UDTO dto,
    CancellationToken cancellationToken)
        {
            await base.BeforeUpdateAsync(entity, dto, cancellationToken);

            if (!CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {entity.Id} was not found.");

            dto.CreatedById = entity.CreatedById;
        }

        protected override async Task BeforeDeleteAsync(
            T entity,
            CancellationToken cancellationToken)
        {
            await base.BeforeDeleteAsync(entity, cancellationToken);

            if (!CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {entity.Id} was not found.");
        }

        protected virtual bool CanModify(T entity)
        {
            return CanAccess(entity.CreatedById);
        }

    }
}