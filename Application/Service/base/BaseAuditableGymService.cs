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
        : BaseService<T, RDTO, CDTO, UDTO>
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

        public override async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            dto.CreatedByPersonId = _currentUser.CurrentPersonId ?? throw new InvalidOperationException("Forbiden: You cannot add record");

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
                    _currentUser.CurrentPersonId);

                throw new NotFoundException($"{typeof(T).Name} with ID {entity.Id} was not found.");
            }

            dto.CreatedByPersonId = entity.CreatedByPersonId;

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
            dto.CreatedByPersonId = _currentUser.CurrentPersonId ?? throw new InvalidOperationException("CurrentPersonId is null. Cannot add entity without a valid person ID.");

            return Task.CompletedTask;
        }

        protected virtual bool CanModify(T entity)
        {
            return CanAccess(entity.CreatedByPersonId);
        }

    }
}