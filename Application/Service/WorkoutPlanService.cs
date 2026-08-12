using System;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    public class WorkoutPlanService : BaseAuditableFileService<WorkoutPlan, WorkoutPlanRDTO, WorkoutPlanCDTO, WorkoutPlanUDTO>, IWorkoutPlanService
    {
        private readonly IUserWorkoutBlockRepo _blockRepo;
        private readonly ICurrentPlanService _currentPlanService;
        private readonly IMemberWorkoutPlanRepo _memberWorkoutPlanRepo;

        public WorkoutPlanService(
            IWorkoutPlanRepo repo,
            IUserWorkoutBlockRepo blockRepo,
            ICurrentPlanService currentPlanService,
            IMemberWorkoutPlanRepo memberWorkoutPlanRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            IStorageService storageService,
            ILogger<WorkoutPlanService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _blockRepo = blockRepo;
            _currentPlanService = currentPlanService;
            _memberWorkoutPlanRepo = memberWorkoutPlanRepo;
        }

        protected override async Task BeforeAddAsync(WorkoutPlanCDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);

            if (!CurrentUser.IsSuperAdmin)
            {
                // Check if user is blocked
                bool isBlocked = await _blockRepo.DbSet.AnyAsync(
                    x => x.BlockedUserId == CurrentUserId && x.BlockedUntil > DateTime.UtcNow,
                    cancellationToken);

                if (isBlocked)
                {
                    throw new ForbiddenException("You are blocked by Admin from creating workout plans.");
                }

                // Check active subscription
                if (!CurrentUser.IsGymOwner)
                {
                    throw new ForbiddenException("Only gym owners with an active subscription can create workout plans.");
                }

                var planResult = await _currentPlanService.GetCurrentPlanAsync(CurrentUserId, cancellationToken);
                if (planResult.IsFree || planResult.SubscriptionStatus != Domain.Enum.OwnerSubscriptionStatus.Active)
                {
                    throw new ForbiddenException("Only gym owners with an active, non-free subscription can create workout plans.");
                }
            }
        }


        protected override async Task BeforeDeleteAsync(WorkoutPlan entity, CancellationToken cancellationToken)
        {

            await base.BeforeDeleteAsync(entity, cancellationToken);

            bool hasActiveAssignments = await _memberWorkoutPlanRepo.DbSet.AnyAsync(
                   x => x.WorkoutPlanId == entity.Id && x.Status == Domain.Enum.MemberWorkoutPlanStatus.Active,
                   cancellationToken);

            if (hasActiveAssignments)
            {
                throw new InvalidOperationException("Cannot delete workout plan because it is currently assigned to active members.");
            }


        }

        public async Task ApproveAsync(int id, CancellationToken cancellationToken)
        {

            var entity = await _repo.GetByIdAsync(id, true, cancellationToken);
            if (entity == null)
            {
                throw new NotFoundException($"Workout plan with ID {id} was not found.");
            }

            entity.IsApproved = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
