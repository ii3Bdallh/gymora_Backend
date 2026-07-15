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

        public override async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding auditable {EntityType} by user {UserId}", typeof(T).Name, CurrentUserId);
            dto.CreatedById = CurrentUserId;

            return await base.AddAsync(dto, cancellationToken);
        }

        protected override Task BeforeUpdateAsync(
            T entity,
            UDTO dto,
            CancellationToken cancellationToken)
        {
            if (!CanModify(entity))
            {
                _logger.LogWarning(
                    "Unauthorized attempt to update {EntityType} with ID {Id} by user {UserId}",
                    typeof(T).Name,
                    entity.Id,
                    CurrentUserId);

                throw new NotFoundException($"{typeof(T).Name} with ID {entity.Id} was not found.");
            }

            dto.CreatedById = entity.CreatedById;

            return Task.CompletedTask;
        }

        protected override Task BeforeDeleteAsync(
            T entity,
            CancellationToken cancellationToken)
        {
            if (!CanModify(entity))
            {
                throw new NotFoundException($"{typeof(T).Name} with ID {entity.Id} was not found.");
            }

            return Task.CompletedTask;
        }

        protected override Task BeforeAddAsync(
    CDTO dto,
    CancellationToken cancellationToken)
        {
            dto.CreatedById = CurrentUserId;

            return Task.CompletedTask;
        }

        protected virtual bool CanModify(T entity)
        {
            return CanAccess(entity.CreatedById);
        }
    }
}

