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
using Domain.Interface;

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

        protected override Task AfterMapReadAsync(T entity, RDTO dto, CancellationToken cancellationToken)
        {
            if (entity is IOnlyMeCanSee &&
   !CanAccess(entity.CreatedById))
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to access this resource.");
            }
            return base.AfterMapReadAsync(entity, dto, cancellationToken);
        }
        protected override Task BeforeAddAsync(CDTO dto, CancellationToken cancellationToken)
        {
            base.BeforeAddAsync(dto, cancellationToken);

            dto.CreatedById = CurrentUserId;

            return Task.CompletedTask;
        }



        protected override Task BeforeUpdateAsync(T entity, UDTO dto, CancellationToken cancellationToken)
        {
            base.BeforeUpdateAsync(entity, dto, cancellationToken);
            if (!CanAccess(entity.CreatedById))
            {
                throw new ForbiddenException("You are not authorized to perform this action.");
            }
            dto.CreatedById = CurrentUserId;
            return Task.CompletedTask;
        }

        protected override Task BeforeDeleteAsync(T entity, CancellationToken cancellationToken)
        {
            base.BeforeDeleteAsync(entity, cancellationToken);
            if (!CanAccess(entity.CreatedById))
            {
                throw new ForbiddenException("You are not authorized to perform this action.");
            }
            return Task.CompletedTask;
        }


        private bool CanAccess(int createdById)
        {
            return HasFullAccess || createdById == CurrentUserId;
        }

    }
}