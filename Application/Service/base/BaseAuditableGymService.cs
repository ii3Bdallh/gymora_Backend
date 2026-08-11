using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Interface;
using Domain.Model.Base;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Service.Base
{
    public abstract class BaseAuditableGymService<T, RDTO, CDTO, UDTO>
        : BaseGymService<T, RDTO, CDTO, UDTO>
        where T : BaseAuditableGymEntity
        where RDTO : BaseGymAuditableRDTO
        where CDTO : BaseGymAuditableCDTO
        where UDTO : BaseGymAuditableUDTO
    {
        protected BaseAuditableGymService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
        }

        private bool CanAccess(int createdByPersonId)
        {
            return HasFullAccess || CurrentUser.IsGymOwner || CurrentUser.IsGymManager || createdByPersonId == (CurrentUser.CurrentPersonId ?? 0);
        }



        protected override Task AfterMapReadAsync(T entity, RDTO dto, CancellationToken cancellationToken)
        {
            if (entity is IOnlyMeCanSeeAtGym &&
   !CanAccess(entity.CreatedByPersonId))
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to access this resource.");
            }
            return base.AfterMapReadAsync(entity, dto, cancellationToken);
        }

        protected override Task BeforeAddAsync(CDTO dto, CancellationToken cancellationToken)
        {
            base.BeforeAddAsync(dto, cancellationToken);

            dto.CreatedByPersonId = CurrentPersonId ?? throw new ForbiddenException("You are not authorized to perform this action.");

            return Task.CompletedTask;
        }



        protected override Task BeforeUpdateAsync(T entity, UDTO dto, CancellationToken cancellationToken)
        {
            base.BeforeUpdateAsync(entity, dto, cancellationToken);
            if (!CanAccess(entity.CreatedByPersonId))
            {
                throw new ForbiddenException("You are not authorized to perform this action.");
            }
            dto.CreatedByPersonId = CurrentUserId;
            return Task.CompletedTask;
        }

        protected override Task BeforeDeleteAsync(T entity, CancellationToken cancellationToken)
        {
            base.BeforeDeleteAsync(entity, cancellationToken);
            if (!CanAccess(entity.CreatedByPersonId))
            {
                throw new ForbiddenException("You are not authorized to perform this action.");
            }
            return Task.CompletedTask;
        }

    }
}