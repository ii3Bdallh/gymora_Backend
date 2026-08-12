using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SessionService : BaseService<Session, SessionRDTO, SessionCDTO, SessionUDTO>, ISessionService
    {
        private readonly IUserWorkoutBlockRepo _blockRepo;
        private readonly ICurrentPlanService _currentPlanService;
        private readonly IWorkoutPlanRepo _workoutPlanRepo;

        public SessionService(
            ISessionRepo repo,
            IUserWorkoutBlockRepo blockRepo,
            ICurrentPlanService currentPlanService,
            IWorkoutPlanRepo workoutPlanRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<SessionService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _blockRepo = blockRepo;
            _currentPlanService = currentPlanService;
            _workoutPlanRepo = workoutPlanRepo;
        }

        private async Task VerifyWorkoutPlanOwnershipAsync(int planId, CancellationToken cancellationToken)
        {
            var plan = await _workoutPlanRepo.GetByIdAsync(planId, false, cancellationToken);
            if (plan == null)
                throw new NotFoundException($"Workout plan with ID {planId} was not found.");

            if (plan.CreatedById != CurrentUserId && !CurrentUser.IsSuperAdmin)
                throw new ForbiddenException("You do not have access to modify sessions in this workout plan.");
        }

        protected override async Task BeforeAddAsync(SessionCDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);
            await VerifyWorkoutPlanOwnershipAsync(dto.WorkoutPlanId, cancellationToken);

            if (!CurrentUser.IsSuperAdmin)
            {
                // Check block
                bool isBlocked = await _blockRepo.DbSet.AnyAsync(
                    x => x.BlockedUserId == CurrentUserId && x.BlockedUntil > DateTime.UtcNow,
                    cancellationToken);

                if (isBlocked)
                {
                    throw new ForbiddenException("You are blocked by SuperAdmin from creating workout sessions.");
                }

                // Check active subscription
                if (!CurrentUser.IsGymOwner)
                {
                    throw new ForbiddenException("Only gym owners with an active subscription can create workout sessions.");
                }

                var planResult = await _currentPlanService.GetCurrentPlanAsync(CurrentUserId, cancellationToken);
                if (planResult.IsFree || planResult.SubscriptionStatus != Domain.Enum.OwnerSubscriptionStatus.Active)
                {
                    throw new ForbiddenException("Only gym owners with an active, non-free subscription can create workout sessions.");
                }
            }
        }



        protected override async Task BeforeDeleteAsync(Session entity, CancellationToken cancellationToken)
        {
            await base.BeforeDeleteAsync(entity, cancellationToken);
            await VerifyWorkoutPlanOwnershipAsync(entity.WorkoutPlanId, cancellationToken);
        }

        protected override async Task AfterMapAddAsync(Session entity, SessionCDTO dto, CancellationToken cancellationToken)
        {
            await base.AfterMapAddAsync(entity, dto, cancellationToken);
            entity.IsApproved = CurrentUser.IsSuperAdmin;
        }

        public async Task ApproveAsync(int id, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(id, true, cancellationToken);
            if (entity == null)
            {
                throw new NotFoundException($"Workout session with ID {id} was not found.");
            }

            entity.IsApproved = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<SessionRDTO>> AddRangeAsync(IEnumerable<SessionCDTO> dtos, CancellationToken cancellationToken)
        {
            // 1. Verify workout plan ownership once per unique planId
            var uniquePlanIds = dtos.Select(d => d.WorkoutPlanId).Distinct().ToList();
            foreach (var planId in uniquePlanIds)
            {
                await VerifyWorkoutPlanOwnershipAsync(planId, cancellationToken);
            }

            // 2. Check blocking status once for the current user
            if (!CurrentUser.IsSuperAdmin)
            {
                bool isBlocked = await _blockRepo.DbSet.AnyAsync(
                    x => x.BlockedUserId == CurrentUserId && x.BlockedUntil > DateTime.UtcNow,
                    cancellationToken);

                if (isBlocked)
                {
                    throw new ForbiddenException("You are blocked by SuperAdmin from creating workout sessions.");
                }
            }

            var addedEntities = new List<Session>();
            foreach (var dto in dtos)
            {
                var entity = _mapper.Map<Session>(dto);
                entity.IsApproved = CurrentUser.IsSuperAdmin;

                var added = await _repo.AddAsync(entity, cancellationToken);
                addedEntities.Add(added);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<IEnumerable<SessionRDTO>>(addedEntities);
        }
    }
}
