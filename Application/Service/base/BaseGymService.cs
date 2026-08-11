using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Model.Base;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Service.Base
{
    public abstract class BaseGymService<T, RDTO, CDTO, UDTO>
        : BaseService<T, RDTO, CDTO, UDTO>
        where T : BaseGymEntity
        where RDTO : BaseGymRDTO
        where CDTO : BaseGymCDTO
        where UDTO : BaseGymUDTO
    {
        protected BaseGymService(
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

        protected override Task BeforeAddAsync(CDTO dto, CancellationToken cancellationToken)
        {
            base.BeforeAddAsync(dto, cancellationToken);

            if (dto.GymId != 0 && !CanAccess(dto.GymId))
            {
                throw new ForbiddenException("You are not authorized to perform this action.");
            }

            dto.GymId = CurrentGymId ?? throw new ForbiddenException("You are not authorized to perform this action.");

            return Task.CompletedTask;
        }

        protected override Task BeforeUpdateAsync(T entity, UDTO dto, CancellationToken cancellationToken)
        {
            base.BeforeUpdateAsync(entity, dto, cancellationToken);
            if (!CanAccess(entity.GymId))
            {
                throw new ForbiddenException("You are not authorized to perform this action.");
            }
            dto.GymId = CurrentGymId ?? throw new ForbiddenException("You are not authorized to perform this action.");
            return Task.CompletedTask;
        }

        protected override Task BeforeDeleteAsync(T entity, CancellationToken cancellationToken)
        {
            base.BeforeDeleteAsync(entity, cancellationToken);
            if (!CanAccess(entity.GymId))
            {
                throw new ForbiddenException("You are not authorized to perform this action.");
            }
            return Task.CompletedTask;
        }

        private bool CanAccess(int gymId)
        {
            return HasFullAccess || gymId == CurrentGymId;
        }

    }
}
