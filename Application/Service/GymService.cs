using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Domain.Enum;
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
using Gymora.Contracts.Authentication;

namespace Application.Service
{
    public class GymService : BaseFileService<Gym, GymRDTO, GymCDTO, GymUDTO>, IGymService
    {

        private readonly ICurrentPlanService _currentPlanService;
        private readonly IGymRepo _gymRepo;
        private readonly IUserRepo _usersRepo;
        private readonly IGymPersonRepo _gymPersonRepo;
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
            IUserRepo usersRepo,
            IGymPersonRepo gymPersonRepo
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _currentPlanService = currentPlanService;
            _usersRepo = usersRepo;
            _gymRepo = repo;
            _gymPersonRepo = gymPersonRepo;
        }
        protected override async Task BeforeAddAsync(GymCDTO dto, CancellationToken cancellationToken)
        {
            bool canCreateNewGym = await _currentPlanService.HasAvailableGymSlotAsync(CurrentUserId, cancellationToken);
            if (!canCreateNewGym)
                throw new InvalidOperationException("You have exceeded the maximum number of gyms allowed for your current subscription plan.");
        }

        protected override async Task AfterMapAddAsync(Gym entity, GymCDTO dto, CancellationToken cancellationToken)
        {
            await base.AfterMapAddAsync(entity, dto, cancellationToken);

            var currentUserObj = await _usersRepo.GetByIdAsync(CurrentUserId, true, cancellationToken);
            if (currentUserObj == null)
                throw new NotFoundException($"User with ID {CurrentUserId} was not found.");

            var ownerPerson = new GymPerson
            {
                Gym = entity,
                UserId = CurrentUserId,
                PersonType = PersonType.Owner,
                Name = currentUserObj.PersonName ?? currentUserObj.UserName ?? currentUserObj.Email ?? "Gym Owner",
                PhoneNumber = currentUserObj.PhoneNumber ?? "0000000000",
                Email = currentUserObj.Email,
                AccessStatus = GymPersonAccessStatus.Active,
                IsActive = true,
                CreatedById = CurrentUserId
            };

            await _gymPersonRepo.AddAsync(ownerPerson, cancellationToken);
        }

        private async Task CheckCanModifyGymAsync(int gymId, CancellationToken ct)
        {
            if (CurrentUser.IsSuperAdmin)
                return;

            var isOwner = CurrentUser.IsGymOwner;

            if (!isOwner)
                throw new UnauthorizedAccessException("You do not have permission to modify this gym.");
        }

        protected override async Task BeforeUpdateAsync(Gym entity, GymUDTO dto, CancellationToken cancellationToken)
        {
            await CheckCanModifyGymAsync(entity.Id, cancellationToken);
        }

        protected override async Task BeforeDeleteAsync(Gym entity, CancellationToken cancellationToken)
        {
            await CheckCanModifyGymAsync(entity.Id, cancellationToken);
        }



        public async Task ChangeOwnerOfGymAsync(int gymId, int newOwnerUserId, CancellationToken ct = default)
        {
            Gym? gym = await _repo.GetByIdAsync(gymId, true, true, ct);

            if (gym == null)
                throw new NotFoundException($"Gym with ID {gymId} was not found.");

            var currentOwnerPerson = await _gymPersonRepo.GetGymOwnerAsync(gymId, ct);

            if (currentOwnerPerson == null)
                throw new NotFoundException($"Owner of gym with ID {gymId} was not found.");

            if (currentOwnerPerson.UserId == newOwnerUserId)
                throw new InvalidOperationException("The new owner is already the current owner of the gym.");

            if (currentOwnerPerson.UserId != CurrentUserId)
                throw new UnauthorizedAccessException("You do not have permission to change the owner of this gym.");

            ApplicationUser? newOwner = await _usersRepo.GetByIdAsync(newOwnerUserId, true, ct);

            if (newOwner == null)
                throw new NotFoundException($"User with ID {newOwnerUserId} was not found.");

            bool canCreateNewGym = await _currentPlanService.HasAvailableGymSlotAsync(newOwner.Id, ct);

            if (!canCreateNewGym)
                throw new InvalidOperationException("The new owner has exceeded the maximum number of gyms allowed for their current subscription plan.");

            // Update old owner record to inactive
            currentOwnerPerson.IsActive = false;

            // Update new owner record if they already have one in GymPerson, or create a new one
            var newOwnerPerson = await _gymPersonRepo.GetGymPersonAsync(gymId, newOwnerUserId, ct);

            if (newOwnerPerson != null)
            {
                newOwnerPerson.PersonType = PersonType.Owner;
                newOwnerPerson.IsActive = true;
                newOwnerPerson.AccessStatus = GymPersonAccessStatus.Active;
            }
            else
            {
                newOwnerPerson = new GymPerson
                {
                    GymId = gymId,
                    UserId = newOwnerUserId,
                    PersonType = PersonType.Owner,
                    Name = newOwner.PersonName ?? newOwner.UserName ?? newOwner.Email ?? "Gym Owner",
                    PhoneNumber = newOwner.PhoneNumber ?? "0000000000",
                    Email = newOwner.Email,
                    AccessStatus = GymPersonAccessStatus.Active,
                    IsActive = true,
                    CreatedById = CurrentUserId
                };
                await _gymPersonRepo.AddAsync(newOwnerPerson, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            await PublishEntityChangedAsync(gym.Id, ct);
        }

        public async Task<UserGymsListRDTO> GetUserGymsAsync(UserGymsPagedReq req, CancellationToken cancellationToken)
        {
            return await _gymRepo.GetUserGymsAsync(CurrentUserId, req, cancellationToken);
        }
    }
}