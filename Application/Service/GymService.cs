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
using Domain.Model.Auth;
using Application.Interface.Repo.Shared;

namespace Application.Service
{
    public class GymService : BaseAuditableFileService<Gym, GymRDTO, GymCDTO, GymUDTO>, IGymService
    {

        private readonly ICurrentPlanService _currentPlanService;

        private readonly IUserRepo _usersRepo;
        public GymService(
            IGymRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<GymService> logger,
            IStorageService storageService,
            ICurrentPlanService currentPlanService,
            IUserRepo usersRepo
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _currentPlanService = currentPlanService;
            _usersRepo = usersRepo;
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

        public async Task ChangeOwnerOfGymAsync(int gymId, int newOwnerUserId, CancellationToken ct = default)
        {
            Gym? gym = await _repo.GetByIdAsync(gymId, true, true, ct);

            if (gym == null)
                throw new NotFoundException($"Gym with ID {gymId} was not found.");

            if (gym.CreatedById == newOwnerUserId)
                throw new InvalidOperationException("The new owner is already the current owner of the gym.");

            if (gym.CreatedById != CurrentUserId)
                throw new UnauthorizedAccessException("You do not have permission to change the owner of this gym.");

            ApplicationUser ? newOwner = await _usersRepo.GetByIdAsync(newOwnerUserId, true, ct);

            if (newOwner == null)
                throw new NotFoundException($"User with ID {newOwnerUserId} was not found.");

            bool canCreateNewGym = await _currentPlanService.HasAvailableGymSlotAsync(newOwner.Id, ct);

            if (!canCreateNewGym)
                throw new InvalidOperationException("The new owner has exceeded the maximum number of gyms allowed for their current subscription plan.");

            gym.CreatedById = newOwnerUserId;

            await _unitOfWork.SaveChangesAsync(ct);

            await PublishEntityChangedAsync(gym.Id, ct);
        }
    }
}