// Application/Service/Base/BaseAuditableService.cs
using Application.DTO;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using AutoMapper;
using Domain.Model.Base;
using MassTransit;
using Application.Cache;
using Application.Interface.Service.Shared;
using Application.Model;
using Domain.Events;
using Microsoft.Extensions.Logging;
using Domain.Interface;

namespace Application.Service.Base
{
    /// <summary>
    /// Base service for entities that are auditable and visible to users within the same gym.
    /// مثال: Gym, Member, WorkoutPlan, Exercise, Revenue, etc.
    /// </summary>
    public abstract class BaseAuditableService<T, RDTO, CDTO, UDTO>
        : BaseService<T, RDTO, CDTO, UDTO>
        where T : BaseAuditableEntity
        where RDTO : BaseAuditableRDTO
        where CDTO : BaseAuditableCDTO
        where UDTO : BaseAuditableUDTO
    {
        protected BaseAuditableService(
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

        // Read Operations: متاحة للكل داخل الجيم



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


        private bool CanAccess(int userId)
        {
            return HasFullAccess || userId == CurrentUserId;
        }

    }
}

