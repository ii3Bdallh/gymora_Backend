using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using Application.Service.Shared;
using AutoMapper;
using Domain.Enum;
using Domain.Model;
using Domain.Model.Auth;
using Gymora.Contracts.Authentication;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            CurrentPlanResult canCreateNew = await _currentPlanService.GetCurrentPlanAsync(CurrentUserId, cancellationToken);
            if (canCreateNew.IsOverMemberLimit)
                throw new InvalidOperationException("You Have Reached Your Member Limit");

            if (canCreateNew.IsOverCoachLimit)
                throw new InvalidOperationException("You Have Reached Your Coach Limit");

            if (canCreateNew.IsOverGymLimit)
                throw new InvalidOperationException("You Have Reached Your Gym Limit");

            dto.OwnerUserId = CurrentUserId;
        }

        protected override async Task AfterMapAddAsync(Gym entity, GymCDTO dto, CancellationToken cancellationToken)
        {
            await base.AfterMapAddAsync(entity, dto, cancellationToken);

            var ownerUser = await _usersRepo.GetByIdAsync(entity.OwnerUserId, true, cancellationToken);
            if (ownerUser == null)
                throw new NotFoundException($"User with ID {entity.OwnerUserId} was not found.");

            var ownerPerson = new GymPerson
            {
                Gym = entity,
                UserId = entity.OwnerUserId,
                PersonType = PersonType.Owner,
                Name = ownerUser.PersonName ?? ownerUser.UserName ?? ownerUser.Email ?? "Gym Owner",
                PhoneNumber = ownerUser.PhoneNumber ?? "0000000000",
                Email = ownerUser.Email,
                AccessStatus = GymPersonAccessStatus.Active,
                CreatedById = CurrentUserId
            };

            await _gymPersonRepo.AddAsync(ownerPerson, cancellationToken);
        }

        private Task CheckCanModifyGymAsync(int gymId, CancellationToken ct)
        {
            if (CurrentUser.IsSuperAdmin)
                return Task.CompletedTask;

            var isOwner = CurrentUser.IsGymOwner;

            if (!isOwner)
                throw new UnauthorizedAccessException("You do not have permission to modify this gym.");

            return Task.CompletedTask;
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
            Gym? gym = await _repo.GetByIdAsync(gymId, true, ct);

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

            CurrentPlanResult canCreateNew = await _currentPlanService.GetCurrentPlanAsync(newOwner.Id, ct);
            if (canCreateNew.IsOverMemberLimit)
                throw new InvalidOperationException("You Have Reached Your Member Limit");

            if (canCreateNew.IsOverCoachLimit)
                throw new InvalidOperationException("You Have Reached Your Coach Limit");

            if (canCreateNew.IsOverGymLimit)
                throw new InvalidOperationException("The new owner has reached their gym limit.");

            // Update Gym entity owner
            gym.OwnerUserId = newOwnerUserId;

            // Delete old owner record from the gym completely
            await _gymPersonRepo.DeleteAsync(currentOwnerPerson, ct);

            // Update new owner record if they already have one in GymPerson, or create a new one
            var newOwnerPerson = await _gymPersonRepo.GetGymPersonAsync(gymId, newOwnerUserId, ct);

            if (newOwnerPerson != null)
            {
                newOwnerPerson.PersonType = PersonType.Owner;
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