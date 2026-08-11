using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Enum;
using Domain.Events;
using Domain.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class InvitationService : BaseGymService<Invitation, InvitationRDTO, InvitationCDTO, InvitationUDTO>, IInvitationService
    {
        private readonly IInvitationRepo _invitationRepo;
        private readonly IGymPersonRepo _gymPersonRepo;

        private readonly IGymRepo _gymRepo;

        private readonly ICurrentPlanService _currentPlanService;

        private readonly IUserRepo _userRepo;

        public InvitationService(
            IInvitationRepo repo,
            IGymPersonRepo gymPersonRepo,
            IUserRepo userRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<InvitationService> logger,
            IGymRepo gymRepo,
            ICurrentPlanService currentPlanService
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _invitationRepo = repo;
            _gymPersonRepo = gymPersonRepo;
            _gymRepo = gymRepo;
            _currentPlanService = currentPlanService;
            _userRepo = userRepo;
        }

        #region Send Invite
        protected override async Task BeforeAddAsync(InvitationCDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);

            int ownerUserId = await _gymRepo.GetOwnerIdAsync(CurrentGymId ?? 0);


            CurrentPlanResult canCreateNew = await _currentPlanService.GetCurrentPlanAsync(ownerUserId, cancellationToken);
            if (canCreateNew.IsOverMemberLimit)
                throw new InvalidOperationException("You Have Reached Your Member Limit");

            if (canCreateNew.IsOverCoachLimit)
                throw new InvalidOperationException("You Have Reached Your Coach Limit");

            if (canCreateNew.IsOverGymLimit)
                throw new InvalidOperationException("You Have Reached Your Gym Limit");


            // 1. Check if the person is already a member of this gym
            var existingPerson = await _gymPersonRepo.GetGymPersonAsync(dto.GymId, dto.UserId, cancellationToken);
            if (existingPerson != null)
                throw new InvalidOperationException("This person is already registered in this gym.");

            // 2. Check if a pending invitation already exists for this user
            var existingPending = await _invitationRepo.HasPendingInvitationAsync(dto.GymId, dto.UserId, cancellationToken);
            if (existingPending)
                throw new InvalidOperationException("An active invitation has already been sent to this user.");
        }

        protected override Task AfterMapAddAsync(Invitation entity, InvitationCDTO dto, CancellationToken cancellationToken)
        {
            // Set auditing and default status — no token needed
            entity.Status = InvitationStatus.Pending;
            entity.CreatedOn = DateTime.UtcNow;
            entity.CreatedByPersonId = CurrentPersonId ?? throw new InvalidOperationException("Current person context is missing.");
            return base.AfterMapAddAsync(entity, dto, cancellationToken);
        }


        public async Task<InvitationRDTO> CreateInvitationAsync(InvitationCDTO dto, CancellationToken ct = default)
        {
            InvitationRDTO invitationRDTO = await base.AddAsync(dto, ct);

            await _publishEndpoint.Publish(new InvitationCreatedEvent
            {
                Id = invitationRDTO.Id,
                GymId = invitationRDTO.GymId,
                UserId = invitationRDTO.UserId,
                GymRole = invitationRDTO.GymRole.ToString(),
                InvitedByUserId = CurrentUserId
            }, ct);

            return invitationRDTO;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────
        // Accept — called by the invited user pressing Accept in the app
        // ─────────────────────────────────────────────────────────────
        public async Task<InvitationRDTO> AcceptInvitationAsync(int invitationId, CancellationToken ct = default)
        {
            int ownerUserId = await _gymRepo.GetOwnerIdAsync(CurrentGymId ?? 0);


            CurrentPlanResult canCreateNew = await _currentPlanService.GetCurrentPlanAsync(ownerUserId, ct);

            if (canCreateNew.IsOverMemberLimit)
                throw new InvalidOperationException("You Have Reached Your Member Limit");

            if (canCreateNew.IsOverCoachLimit)
                throw new InvalidOperationException("You Have Reached Your Coach Limit");

            if (canCreateNew.IsOverGymLimit)
                throw new InvalidOperationException("You Have Reached Your Gym Limit");

            var invitation = await _invitationRepo.GetByIdIgnoringSecurityAsync(invitationId, true, ct,
                include: q => q.Include(x => x.CreatedByPerson));
            if (invitation == null)
                throw new NotFoundException("Invitation was not found.");

            if (invitation.Status != InvitationStatus.Pending)
                throw new InvalidOperationException($"This invitation cannot be accepted because its status is {invitation.Status}.");

            var acceptingUser = await _userRepo.GetByIdAsync(CurrentUserId, false, ct);
            if (acceptingUser == null)
                throw new NotFoundException("User was not found.");

            if (invitation.UserId != CurrentUserId)
                throw new ForbiddenException("This invitation was not sent to you.");

            // Make sure the user is not already in the gym
            var existingPerson = await _gymPersonRepo.GetGymPersonAsync(invitation.GymId, CurrentUserId, ct);
            if (existingPerson != null)
                throw new InvalidOperationException("You are already registered in this gym.");

            // Update invitation
            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            await _invitationRepo.UpdateAsync(invitation, ct);

            // Auto-create GymPerson + Profile using values stored in the invitation
            var personType = invitation.GymRole == GymRole.Member ? PersonType.Member : PersonType.Staff;
            var gymPerson = new GymPerson
            {
                GymId = invitation.GymId,
                UserId = CurrentUserId,
                PersonType = personType,
                Name = acceptingUser.PersonName ?? acceptingUser.UserName ?? acceptingUser.Email ?? "Gym Person",
                PhoneNumber = acceptingUser.PhoneNumber ?? "0000000000",
                Email = acceptingUser.Email,
                AccessStatus = GymPersonAccessStatus.Active,
                CreatedById = CurrentUserId,
                CreatedOn = DateTime.UtcNow
            };

            if (personType == PersonType.Member)
            {
                // Use membership snapshot from the invitation
                var startDate = DateTime.UtcNow;
                var durationDays = invitation.DurationDays ?? 30;
                var finalAmount = (invitation.PricePaid ?? 0) - (invitation.DiscountAmount ?? 0);

                gymPerson.MemberProfile = new GymMemberProfile
                {
                    MembershipPlanId = invitation.MembershipPlanId,
                    PlanName = invitation.PlanName ?? "Basic",
                    DurationDays = durationDays,
                    PricePaid = invitation.PricePaid ?? 0,
                    DiscountAmount = invitation.DiscountAmount ?? 0,
                    FinalAmount = finalAmount,
                    MembershipStartDate = startDate,
                    MembershipEndDate = startDate.AddDays(durationDays)
                };

                await _gymPersonRepo.AddAsync(gymPerson, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                // Publish event → Finance module will create a Revenue record when ready
                await _publishEndpoint.Publish(new MembershipCreatedEvent
                {
                    GymPersonId = gymPerson.Id,
                    GymId = invitation.GymId,
                    MemberUserId = CurrentUserId,
                    PlanName = gymPerson.MemberProfile.PlanName,
                    DurationDays = durationDays,
                    PricePaid = gymPerson.MemberProfile.PricePaid,
                    DiscountAmount = gymPerson.MemberProfile.DiscountAmount,
                    FinalAmount = finalAmount,
                    MembershipStartDate = startDate,
                    MembershipEndDate = gymPerson.MemberProfile.MembershipEndDate!.Value,
                    CreatedByUserId = invitation.CreatedByPerson?.UserId ?? 0
                }, ct);
            }
            else if (personType == PersonType.Staff)
            {
                // Use salary snapshot from the invitation
                gymPerson.StaffProfile = new GymStaffProfile
                {
                    GymRoleId = invitation.GymRole,
                    Salary = invitation.Salary ?? 0,
                    SalaryValidFrom = invitation.SalaryValidFrom ?? DateTime.UtcNow,
                    SalaryValidUntil = invitation.SalaryValidUntil ?? DateTime.UtcNow.AddMonths(1)
                };

                await _gymPersonRepo.AddAsync(gymPerson, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                await _publishEndpoint.Publish(new SalaryPaidEvent
                {
                    GymPersonId = gymPerson.Id,
                    GymId = invitation.GymId,
                    StaffUserId = CurrentUserId,
                    Amount = invitation.Salary ?? 0,
                    PaidAt = DateTime.UtcNow,
                    PeriodFrom = invitation.SalaryValidFrom ?? DateTime.UtcNow,
                    PeriodTo = invitation.SalaryValidUntil ?? DateTime.UtcNow.AddMonths(1),
                    PaidByUserId = invitation.CreatedByPerson?.UserId ?? 0
                }, ct);

            }

            return _mapper.Map<InvitationRDTO>(invitation);
        }

        // ─────────────────────────────────────────────────────────────
        // Reject — called by the invited user pressing Decline in the app
        // ─────────────────────────────────────────────────────────────
        public async Task<InvitationRDTO> RejectInvitationAsync(int invitationId, CancellationToken ct = default)
        {
            var invitation = await _invitationRepo.GetByIdIgnoringSecurityAsync(invitationId, true, ct,
                include: q => q.Include(x => x.CreatedByPerson));
            if (invitation == null)
                throw new NotFoundException("Invitation was not found.");

            if (invitation.Status != InvitationStatus.Pending)
                throw new InvalidOperationException($"This invitation cannot be rejected because its status is {invitation.Status}.");

            var acceptingUser = await _userRepo.GetByIdAsync(CurrentUserId, false, ct);
            if (acceptingUser == null)
                throw new NotFoundException("User was not found.");

            if (invitation.UserId != CurrentUserId)
                throw new ForbiddenException("This invitation was not sent to you.");

            invitation.Status = InvitationStatus.Rejected;
            invitation.RejectedAt = DateTime.UtcNow;

            await _invitationRepo.UpdateAsync(invitation, ct);
            await _unitOfWork.SaveChangesAsync(ct);


            return _mapper.Map<InvitationRDTO>(invitation);
        }

        // ─────────────────────────────────────────────────────────────
        // Cancel — called by Owner/Manager to withdraw the invitation
        // ─────────────────────────────────────────────────────────────
        public async Task<InvitationRDTO> CancelInvitationAsync(int invitationId, CancellationToken ct = default)
        {
            var invitation = await _invitationRepo.GetByIdAsync(invitationId, true, ct);
            if (invitation == null)
                throw new NotFoundException($"Invitation with ID {invitationId} was not found.");

            if (!CanAccess(invitation.GymId))
                throw new ForbiddenException("You are not authorized to cancel invitations for this gym.");

            if (invitation.Status != InvitationStatus.Pending)
                throw new InvalidOperationException($"Cannot cancel invitation because its status is {invitation.Status}.");

            invitation.Status = InvitationStatus.Cancelled;

            await _invitationRepo.UpdateAsync(invitation, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _mapper.Map<InvitationRDTO>(invitation);
        }



    }
}
