using System;
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
using Domain.Enum;
using Domain.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    public class MemberWorkoutPlanService : BaseAuditableGymService<MemberWorkoutPlan, MemberWorkoutPlanRDTO, MemberWorkoutPlanCDTO, MemberWorkoutPlanUDTO>, IMemberWorkoutPlanService
    {
        private readonly IGymPersonRepo _gymPersonRepo;
        private readonly IWorkoutPlanRepo _workoutPlanRepo;

        public MemberWorkoutPlanService(
            IMemberWorkoutPlanRepo repo,
            IGymPersonRepo gymPersonRepo,
            IWorkoutPlanRepo workoutPlanRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<MemberWorkoutPlanService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymPersonRepo = gymPersonRepo;
            _workoutPlanRepo = workoutPlanRepo;
        }

        protected override async Task BeforeAddAsync(MemberWorkoutPlanCDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);

            // 1. Validate WorkoutPlan exists
            var plan = await _workoutPlanRepo.GetByIdAsync(dto.WorkoutPlanId, false, cancellationToken);
            if (plan == null)
                throw new NotFoundException($"Workout plan with ID {dto.WorkoutPlanId} was not found.");

            // 2. Validate Member
            var member = await _gymPersonRepo.GetByIdAsync(dto.MemberId, false, cancellationToken);
            if (member == null)
                throw new NotFoundException($"Member with ID {dto.MemberId} was not found.");

            if (member.GymId != (CurrentGymId ?? 0))
                throw new InvalidOperationException("The specified member does not belong to this gym.");

            if (member.PersonType != PersonType.Member)
                throw new InvalidOperationException($"The person with ID {dto.MemberId} is not a Member.");
        }

        protected override async Task BeforeUpdateAsync(MemberWorkoutPlan entity, MemberWorkoutPlanUDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeUpdateAsync(entity, dto, cancellationToken);

            // Validate WorkoutPlan
            var plan = await _workoutPlanRepo.GetByIdAsync(dto.WorkoutPlanId, false, cancellationToken);
            if (plan == null)
                throw new NotFoundException($"Workout plan with ID {dto.WorkoutPlanId} was not found.");

            // Validate Member
            var member = await _gymPersonRepo.GetByIdAsync(dto.MemberId, false, cancellationToken);
            if (member == null || member.GymId != (CurrentGymId ?? 0) || member.PersonType != PersonType.Member)
                throw new InvalidOperationException("Invalid member specified.");
        }

        public async Task CancelAssignmentAsync(int memberWorkoutPlanId, CancellationToken ct)
        {
            var assignment = await _repo.GetByIdAsync(memberWorkoutPlanId, true, ct);
            if (assignment == null)
                throw new NotFoundException($"Workout assignment with ID {memberWorkoutPlanId} was not found.");

            if (assignment.GymId != (CurrentGymId ?? 0) && !CurrentUser.IsSuperAdmin)
                throw new ForbiddenException("You are not authorized to access this assignment.");

            // Delete the assignment record entirely when cancelled
            await _repo.DeleteAsync(assignment, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
