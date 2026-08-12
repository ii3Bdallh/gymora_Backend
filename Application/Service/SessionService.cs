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
    public class SessionService : BaseService<Session, SessionRDTO, SessionCDTO, SessionUDTO>, ISessionService
    {
        private readonly ISessionExerciseRepo _sessionExerciseRepo;
        private readonly IExerciseRepo _exerciseRepo;
        private readonly IUserWorkoutBlockRepo _blockRepo;
        private readonly ICurrentPlanService _currentPlanService;

        public SessionService(
            ISessionRepo repo,
            ISessionExerciseRepo sessionExerciseRepo,
            IExerciseRepo exerciseRepo,
            IUserWorkoutBlockRepo blockRepo,
            ICurrentPlanService currentPlanService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<SessionService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _sessionExerciseRepo = sessionExerciseRepo;
            _exerciseRepo = exerciseRepo;
            _blockRepo = blockRepo;
            _currentPlanService = currentPlanService;
        }

        protected override async Task BeforeAddAsync(SessionCDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);

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

        protected override async Task AfterMapAddAsync(Session entity, SessionCDTO dto, CancellationToken cancellationToken)
        {
            await base.AfterMapAddAsync(entity, dto, cancellationToken);
            entity.IsApproved = CurrentUser.IsSuperAdmin;
        }




        public async Task<SessionExerciseRDTO> AddExerciseToSessionAsync(int sessionId, SessionExerciseCDTO dto, CancellationToken ct)
        {
            var session = await _repo.GetByIdAsync(sessionId, false, ct);
            if (session == null)
                throw new NotFoundException($"Session with ID {sessionId} was not found.");

            // Enforce that only creator or SuperAdmin can add exercises to this session
            if (session.CreatedById != CurrentUserId && !CurrentUser.IsSuperAdmin)
                throw new ForbiddenException("You do not have access to modify this session.");

            var ex = _mapper.Map<SessionExercise>(dto);
            ex.SessionId = sessionId;

            if (ex.ExerciseId.HasValue && string.IsNullOrEmpty(ex.ExerciseName))
            {
                var exercise = await _exerciseRepo.GetByIdAsync(ex.ExerciseId.Value, false, ct);
                if (exercise != null)
                {
                    ex.ExerciseName = exercise.Name;
                }
            }

            ex = await _sessionExerciseRepo.AddAsync(ex, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _mapper.Map<SessionExerciseRDTO>(ex);
        }

        public async Task RemoveExerciseFromSessionAsync(int sessionId, int exerciseId, CancellationToken ct)
        {
            var session = await _repo.GetByIdAsync(sessionId, false, ct);
            if (session == null)
                throw new NotFoundException($"Session with ID {sessionId} was not found.");

            // Enforce access control
            if (session.CreatedById != CurrentUserId && !CurrentUser.IsSuperAdmin)
                throw new ForbiddenException("You do not have access to modify this session.");

            var ex = await _sessionExerciseRepo.GetByIdAsync(exerciseId, false, ct);
            if (ex == null || ex.SessionId != sessionId)
                throw new NotFoundException($"Exercise with ID {exerciseId} was not found under session {sessionId}.");

            await _sessionExerciseRepo.DeleteAsync(ex, ct);
            await _unitOfWork.SaveChangesAsync(ct);
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
    }
}
