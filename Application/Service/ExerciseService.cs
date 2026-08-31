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
    public class ExerciseService : BaseAuditableFileService<Exercise, ExerciseRDTO, ExerciseCDTO, ExerciseUDTO>, IExerciseService
    {
        private readonly ISessionExerciseRepo _sessionExerciseRepo;
        private readonly IUserWorkoutBlockRepo _blockRepo;
        private readonly ICurrentPlanService _currentPlanService;

        public ExerciseService(
            IExerciseRepo repo,
            ISessionExerciseRepo sessionExerciseRepo,
            IUserWorkoutBlockRepo blockRepo,
            ICurrentPlanService currentPlanService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            IStorageService storageService,
            ILogger<ExerciseService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _sessionExerciseRepo = sessionExerciseRepo;
            _blockRepo = blockRepo;
            _currentPlanService = currentPlanService;
        }

        protected override async Task BeforeAddAsync(ExerciseCDTO dto, CancellationToken cancellationToken)
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
                    throw new ForbiddenException("You are blocked by SuperAdmin from creating workout items.");
                }

                // Check active subscription
                if (!CurrentUser.IsGymOwner)
                {
                    throw new ForbiddenException("Only gym owners with an active subscription can create workout items.");
                }

                var planResult = await _currentPlanService.GetCurrentPlanAsync(CurrentUserId, cancellationToken);
                if (planResult.IsFree || planResult.SubscriptionStatus != Domain.Enum.OwnerSubscriptionStatus.Active)
                {
                    throw new ForbiddenException("Only gym owners with an active, non-free subscription can create workout items.");
                }
            }
        }

        protected override async Task AfterMapAddAsync(Exercise entity, ExerciseCDTO dto, CancellationToken cancellationToken)
        {
            await base.AfterMapAddAsync(entity, dto, cancellationToken);
            entity.IsApproved = CurrentUser.IsSuperAdmin;
        }



        protected override async Task BeforeDeleteAsync(Exercise entity, CancellationToken cancellationToken)
        {
            await base.BeforeDeleteAsync(entity, cancellationToken);

            // Check if exercise is used in sessions
            bool isUsed = await _sessionExerciseRepo.DbSet.AnyAsync(
                e => e.ExerciseId == entity.Id,
                cancellationToken);

            if (isUsed)
            {
                throw new InvalidOperationException("Cannot delete this exercise because it is currently used in workout sessions.");
            }
        }

        public async Task ApproveAsync(int id, CancellationToken cancellationToken)
        {
            if (!CurrentUser.IsSuperAdmin)
            {
                throw new ForbiddenException("Only SuperAdmins are allowed to approve exercises.");
            }

            var entity = await _repo.GetByIdAsync(id, true, cancellationToken);
            if (entity == null)
            {
                throw new NotFoundException($"Exercise with ID {id} was not found.");
            }

            entity.IsApproved = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
