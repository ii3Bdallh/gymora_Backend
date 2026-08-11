using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class RevenueService : BaseGymService<Revenue, RevenueRDTO, RevenueCDTO, RevenueUDTO>, IRevenueService
    {
        private readonly IGymPersonRepo _gymPersonRepo;

        public RevenueService(
            IRevenueRepo repo,
            IGymPersonRepo gymPersonRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<RevenueService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymPersonRepo = gymPersonRepo;
        }

        protected override async Task BeforeAddAsync(RevenueCDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);
            await ValidateRevenuePayloadAsync(dto.GymMemberId, cancellationToken);
        }

        protected override async Task BeforeUpdateAsync(Revenue entity, RevenueUDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeUpdateAsync(entity, dto, cancellationToken);
            await ValidateRevenuePayloadAsync(dto.GymMemberId, cancellationToken);
        }

        private async Task ValidateRevenuePayloadAsync(int? memberId, CancellationToken ct)
        {
            // Validate member if provided
            if (memberId.HasValue)
            {
                var member = await _gymPersonRepo.GetByIdAsync(memberId.Value, false, ct);
                if (member == null)
                    throw new NotFoundException($"Gym member with ID {memberId.Value} was not found.");

                if (member.GymId != (CurrentGymId ?? 0))
                    throw new InvalidOperationException("The specified member does not belong to this gym.");
            }
        }
    }
}
