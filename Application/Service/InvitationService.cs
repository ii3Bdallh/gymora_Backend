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
using Domain.Model.Auth;
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
        private readonly IUserRepo _userRepo;
        private readonly IMembershipPlanRepo _membershipPlanRepo;
        private readonly IGymRepo _gymRepo;
        private readonly ICurrentPlanService _currentPlanService;

        public InvitationService(
            IInvitationRepo repo,
            IGymPersonRepo gymPersonRepo,
            IUserRepo userRepo,
            IMembershipPlanRepo membershipPlanRepo,
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
            _userRepo = userRepo;
            _membershipPlanRepo = membershipPlanRepo;
            _gymRepo = gymRepo;
            _currentPlanService = currentPlanService;
        }

        #region Send Invite

        protected override async Task AfterMapAddAsync(Invitation entity, InvitationCDTO dto, CancellationToken cancellationToken)
        {
            entity.Status = InvitationStatus.Pending;
            entity.CreatedOn = DateTime.UtcNow;
            entity.CreatedByPersonId = CurrentPersonId ?? throw new InvalidOperationException("Current person context is missing.");

            if (dto.GymRole == GymRole.Member && dto.Membership != null)
            {
                var plan = await _membershipPlanRepo.GetByIdAsync(dto.Membership.MembershipPlanId!.Value, false, cancellationToken);
                if (plan != null)
                {
                    entity.PlanName = plan.Name;
                    entity.DurationDays = plan.DurationDays;
                    entity.Amount = plan.Price;
                    entity.DiscountAmount = dto.Membership.DiscountAmount;
                    entity.FinalAmount = plan.Price - dto.Membership.DiscountAmount;
                }
            }

            await base.AfterMapAddAsync(entity, dto, cancellationToken);
        }

        public async Task<InvitationRDTO> CreateInvitationAsync(InvitationCDTO dto, CancellationToken ct = default)
        {
            // 0. Verify that the user exists on the platform
            var user = await _userRepo.GetByIdAsync(dto.UserId, false, ct);
            if (user == null)
                throw new NotFoundException($"User with ID {dto.UserId} was not found.");

            int ownerUserId = await _gymRepo.GetOwnerIdAsync(CurrentGymId ?? 0);

            CurrentPlanResult planResult = await _currentPlanService.GetCurrentPlanAsync(ownerUserId, ct);
            ValidateGymPlanLimits(planResult, dto.GymRole);

            // 1. Check if the person is already a member of this gym
            var existingPerson = await _gymPersonRepo.GetGymPersonAsync(dto.GymId, dto.UserId, ct);
            if (existingPerson != null)
                throw new InvalidOperationException("This person is already registered in this gym.");

            // 2. Check if a pending invitation already exists for this user
            var existingPending = await _invitationRepo.HasPendingInvitationAsync(dto.GymId, dto.UserId, ct);
            if (existingPending)
                throw new InvalidOperationException("An active invitation has already been sent to this user.");

            // 3. Validate membership plan details if inviting a Member
            if (dto.GymRole == GymRole.Member)
            {
                var plan = await _membershipPlanRepo.GetByIdAsync(dto.Membership!.MembershipPlanId!.Value, false, ct);
                if (plan == null)
                    throw new NotFoundException($"Membership plan with ID {dto.Membership.MembershipPlanId.Value} was not found.");

                if (plan.GymId != CurrentGymId)
                    throw new InvalidOperationException("The specified membership plan does not belong to this gym.");
            }

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
            var (invitation, acceptingUser) = await GetAndValidateInvitationForRecipientAsync(invitationId, "accepted", ct);

            // Make sure the user is not already in the gym
            var existingPerson = await _gymPersonRepo.GetGymPersonAsync(invitation.GymId, CurrentUserId, ct);
            if (existingPerson != null)
                throw new InvalidOperationException("You are already registered in this gym.");

            int ownerUserId = await _gymRepo.GetOwnerIdAsync(invitation.GymId);
            CurrentPlanResult planResult = await _currentPlanService.GetCurrentPlanAsync(ownerUserId, ct);
            ValidateGymPlanLimits(planResult, invitation.GymRole);

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
                var finalAmount = invitation.FinalAmount ?? ((invitation.Amount ?? 0) - (invitation.DiscountAmount ?? 0));

                gymPerson.MemberProfile = new GymMemberProfile
                {
                    MembershipPlanId = invitation.MembershipPlanId,
                    PlanName = invitation.PlanName ?? "Basic",
                    DurationDays = durationDays,
                    PricePaid = invitation.Amount ?? 0,
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
            var (invitation, _) = await GetAndValidateInvitationForRecipientAsync(invitationId, "rejected", ct);

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

            if (invitation.GymId != CurrentGymId)
                throw new ForbiddenException("You are not authorized to cancel invitations for this gym.");

            if (invitation.Status != InvitationStatus.Pending)
                throw new InvalidOperationException($"Cannot cancel invitation because its status is {invitation.Status}.");

            invitation.Status = InvitationStatus.Cancelled;

            await _invitationRepo.UpdateAsync(invitation, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _mapper.Map<InvitationRDTO>(invitation);
        }

        // ─────────────────────────────────────────────────────────────
        // Helper Methods
        // ─────────────────────────────────────────────────────────────
        private void ValidateGymPlanLimits(CurrentPlanResult plan, GymRole role)
        {
            if (role == GymRole.Member && plan.IsOverMemberLimit)
                throw new InvalidOperationException("You Have Reached Your Member Limit");

            if (role == GymRole.Coach && plan.IsOverCoachLimit)
                throw new InvalidOperationException("You Have Reached Your Coach Limit");
        }

        private async Task<(Invitation invitation, ApplicationUser acceptingUser)> GetAndValidateInvitationForRecipientAsync(
            int invitationId, string action, CancellationToken ct)
        {
            var invitation = await _invitationRepo.GetByIdIgnoringSecurityAsync(invitationId, true, ct,
                include: q => q.Include(x => x.CreatedByPerson));
            if (invitation == null)
                throw new NotFoundException("Invitation was not found.");

            if (invitation.Status != InvitationStatus.Pending)
                throw new InvalidOperationException($"This invitation cannot be {action} because its status is {invitation.Status}.");

            var acceptingUser = await _userRepo.GetByIdAsync(CurrentUserId, false, ct);
            if (acceptingUser == null)
                throw new NotFoundException("User was not found.");

            if (invitation.UserId != CurrentUserId)
                throw new ForbiddenException("This invitation was not sent to you.");

            return (invitation, acceptingUser);
        }
    }
}
