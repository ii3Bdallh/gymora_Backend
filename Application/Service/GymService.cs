using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Service.Base;


using Application.DTO.Model;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;
using Application.DTO.Exceptions;
using Application.DTO;

namespace Application.Service
{
    public class GymService : BaseAuditableFileService<Gym, GymRDTO, GymCDTO, GymUDTO>, IGymService
    {

        private readonly ICurrentPlanService _currentPlanService;
        public GymService(
            IGymRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<GymService> logger,
            IStorageService storageService,
            ICurrentPlanService currentPlanService
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _currentPlanService = currentPlanService;
        }
        protected override async Task BeforeAddAsync(GymCDTO dto, CancellationToken cancellationToken)
        {
            dto.CreatedById = CurrentUserId;

            bool canCreateNewGym = await _currentPlanService.HasAvailableGymSlotAsync(dto.CreatedById, cancellationToken);
            if (!canCreateNewGym)
                throw new InvalidOperationException("You have exceeded the maximum number of gyms allowed for your current subscription plan.");

        }

        public async Task<LoginResDto> SwitchGymAsync(
    int gymId,
    CancellationToken ct = default)
        {
            throw new NotImplementedException();

        }
    }
}