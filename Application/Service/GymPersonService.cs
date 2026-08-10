using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Service.Base;
using Domain.Events;
using Domain.Enum;
using Application.DTO.Model;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;
using Application.DTO.Exceptions;

namespace Application.Service
{
    public class GymPersonService : BaseGymService<GymPerson, GymPersonRDTO, GymPersonCDTO, GymPersonUDTO>, IGymPersonService
    {
        private readonly IGymPersonRepo _gymPersonRepo;
        private readonly ICurrentPlanService _currentPlanService;

        private readonly IMembershipPlanService _membershipPlanService;

        private readonly IGymRepo _gymRepo;

        public GymPersonService(
            IGymPersonRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<GymPersonService> logger,
            ICurrentPlanService currentPlanService,
            IMembershipPlanService membershipPlanService,
            IGymRepo gymRepo
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymPersonRepo = repo;
            _currentPlanService = currentPlanService;
            _membershipPlanService = membershipPlanService;
            _gymRepo = gymRepo;
        }

        protected override async Task BeforeAddAsync(GymPersonCDTO dto, CancellationToken cancellationToken)
        {
            int ownerUserId = await _gymRepo.GetOwnerIdAsync(CurrentGymId ?? 0);

            //if (dto.PersonType == PersonType.Staff || dto.PersonType == PersonType.StaffMember)
            //{
            //    bool canCreateNewStaff = await _currentPlanService.HasAvailableCoachSlotAsync(ownerUserId, cancellationToken);
            //    if (!canCreateNewStaff)
            //        throw new InvalidOperationException("You have exceeded the maximum number of staffs allowed for your current subscription plan.");
            //}

            //if (dto.PersonType == PersonType.Member || dto.PersonType == PersonType.StaffMember)
            //{
            //    bool canCreateNewMember = await _currentPlanService.HasAvailableMemberSlotAsync(ownerUserId, cancellationToken);
            //    if (!canCreateNewMember)
            //        throw new InvalidOperationException("You have exceeded the maximum number of members allowed for your current subscription plan.");
            //}
            CurrentPlanResult canCreateNew = await _currentPlanService.GetCurrentPlanAsync(ownerUserId, cancellationToken);
            if (canCreateNew.IsOverMemberLimit)
                throw new InvalidOperationException("You Have Reached Your Member Limit");

            if (canCreateNew.IsOverCoachLimit)
                throw new InvalidOperationException("You Have Reached Your Coach Limit");

            if (canCreateNew.IsOverGymLimit)
                throw new InvalidOperationException("You Have Reached Your Gym Limit");

        }

        protected override async Task BeforeUpdateAsync(GymPerson entity, GymPersonUDTO dto, CancellationToken cancellationToken)
        {
            int ownerUserId = await _gymRepo.GetOwnerIdAsync(CurrentGymId ?? 0);


            CurrentPlanResult canCreateNew = await _currentPlanService.GetCurrentPlanAsync(ownerUserId, cancellationToken);
            if (canCreateNew.IsOverMemberLimit)
                throw new InvalidOperationException("You Have Reached Your Member Limit");

            if (canCreateNew.IsOverCoachLimit)
                throw new InvalidOperationException("You Have Reached Your Coach Limit");

            if (canCreateNew.IsOverGymLimit)
                throw new InvalidOperationException("You Have Reached Your Gym Limit");
        }


        public async Task<GymPersonRDTO> LinkAccountToGymAsync(int gymId, Guid inviteCode, CancellationToken ct = default)
        {
            var gymPerson = await _gymPersonRepo.LinkAccountToGymAsync(gymId, inviteCode, ct);

            if (gymPerson is null)
                throw new InvalidOperationException("Failed to link account to gym.");
            await _unitOfWork.SaveChangesAsync(ct);
            var result = _mapper.Map<GymPersonRDTO>(gymPerson);

            return result;
        }

        protected override Task AfterMapAddAsync(GymPerson entity, GymPersonCDTO dto, CancellationToken cancellationToken)
        {
            if (entity.PersonType == PersonType.Staff)
            {
                entity.MemberProfile = null;
            }
            else if (entity.PersonType == PersonType.Member)
            {
                entity.StaffProfile = null;
            }

            return Task.CompletedTask;
        }

        protected override Task AfterMapUpdateAsync(GymPerson entity, GymPersonUDTO dto, CancellationToken cancellationToken)
        {
            // Handle StaffProfile transition/update
            if (entity.PersonType == PersonType.Staff || entity.PersonType == PersonType.StaffMember)
            {
                if (dto.StaffProfile != null)
                {
                    if (entity.StaffProfile == null)
                    {
                        entity.StaffProfile = _mapper.Map<GymStaffProfile>(dto.StaffProfile);
                        entity.StaffProfile.Id = entity.Id; // Ensure PK matches parent
                    }
                    else
                    {
                        _mapper.Map(dto.StaffProfile, entity.StaffProfile);
                    }
                }
            }
            else
            {
                entity.StaffProfile = null; // Will trigger cascade delete
            }

            // Handle MemberProfile transition/update
            if (entity.PersonType == PersonType.Member || entity.PersonType == PersonType.StaffMember)
            {
                if (dto.MemberProfile != null)
                {
                    if (entity.MemberProfile == null)
                    {
                        entity.MemberProfile = _mapper.Map<GymMemberProfile>(dto.MemberProfile);
                        entity.MemberProfile.Id = entity.Id; // Ensure PK matches parent
                    }
                    else
                    {
                        _mapper.Map(dto.MemberProfile, entity.MemberProfile);
                    }
                }
            }
            else
            {
                entity.MemberProfile = null; // Will trigger cascade delete
            }

            return Task.CompletedTask;
        }

        public async Task PaySalaryAsync(int staffId, DateTime? salaryValidFrom, DateTime? salaryValidUntil, CancellationToken ct = default)
        {
            var person = await _repo.GetByIdAsync(staffId, trackChanges: true, cancellationToken: ct);
            if (person == null)
            {
                throw new KeyNotFoundException($"GymPerson with ID {staffId} not found or is inactive.");
            }

            if (person.PersonType != PersonType.Staff && person.PersonType != PersonType.StaffMember)
            {
                throw new InvalidOperationException($"GymPerson with ID {staffId} is not registered as a staff member.");
            }

            if (person.StaffProfile == null)
            {
                throw new InvalidOperationException($"GymPerson with ID {staffId} does not have a staff profile configured.");
            }

            person.StaffProfile.SalaryValidFrom = salaryValidFrom;
            person.StaffProfile.SalaryValidUntil = salaryValidUntil;

            if (person.StaffProfile.Salary == null || person.StaffProfile.Salary <= 0)
            {
                throw new InvalidOperationException($"GymStaff with ID {staffId} does not have a valid salary configured.");
            }

            var now = DateTime.UtcNow;
            if (person.StaffProfile.SalaryValidFrom.HasValue && now < person.StaffProfile.SalaryValidFrom.Value)
            {
                throw new InvalidOperationException($"Salary for GymStaff {staffId} is not valid yet (Valid from: {person.StaffProfile.SalaryValidFrom.Value}).");
            }

            if (person.StaffProfile.SalaryValidUntil.HasValue && now > person.StaffProfile.SalaryValidUntil.Value)
            {
                throw new InvalidOperationException($"Salary for GymStaff {staffId} has expired (Expired on: {person.StaffProfile.SalaryValidUntil.Value}).");
            }

            await _repo.UpdateAsync(person, ct);
            await base.PublishEntityChangedAsync(staffId, ct);

            // Publish SalaryPaidEvent
            await _publishEndpoint.Publish(new SalaryPaidEvent(
                person.Id,
                person.StaffProfile.Salary.Value,
                now,
                person.GymId
            ), ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task<GymPersonRDTO> RenewMemberSubscriptionAsync(int memberId, RenewMembershipDTO dto, CancellationToken ct = default)
        {
            var member = await _gymPersonRepo.GetByIdAsync(memberId, trackChanges: true, cancellationToken: ct);
            if (member == null || (member.PersonType != PersonType.Member && member.PersonType != PersonType.StaffMember))
                throw new NotFoundException($"Member with ID {memberId} was not found.");

            var plan = await _membershipPlanService.GetByIdAsync(dto.MembershipPlanId, false, ct);
            if (plan == null)
                throw new NotFoundException($"Membership plan with ID {dto.MembershipPlanId} was not found.");

            if (member.MemberProfile == null)
            {
                member.MemberProfile = new GymMemberProfile();
            }

            var now = DateTime.UtcNow;
            var currentEndDate = member.MemberProfile.MembershipEndDate;

            member.MemberProfile.MembershipPlanId = plan.Id;
            member.MemberProfile.PlanName = plan.Name;
            member.MemberProfile.DurationDays = plan.DurationDays;
            member.MemberProfile.PricePaid = dto.PricePaid;
            member.MemberProfile.DiscountAmount = dto.DiscountAmount;
            member.MemberProfile.FinalAmount = dto.FinalAmount;
            member.MemberProfile.Notes = dto.Notes;

            if (currentEndDate.HasValue && currentEndDate.Value > now)
            {
                member.MemberProfile.MembershipEndDate = currentEndDate.Value.AddDays(plan.DurationDays);
            }
            else
            {
                member.MemberProfile.MembershipStartDate = now;
                member.MemberProfile.MembershipEndDate = now.AddDays(plan.DurationDays);
            }

            await _gymPersonRepo.UpdateAsync(member, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _mapper.Map<GymPersonRDTO>(member);
        }

        public async Task<GymPersonRDTO> UpdateAccessStatusAsync(int id, UpdateAccessStatusDTO dto, CancellationToken ct = default)
        {
            var member = await _gymPersonRepo.GetByIdAsync(id, trackChanges: true, cancellationToken: ct);
            if (member == null)
                throw new NotFoundException($"GymPerson with ID {id} was not found.");

            member.AccessStatus = dto.Status;

            await _gymPersonRepo.UpdateAsync(member, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _mapper.Map<GymPersonRDTO>(member);
        }

        public async Task LeaveGymAsync(int gymId, CancellationToken ct = default)
        {
            var person = await _gymPersonRepo.GetGymPersonAsync(gymId, CurrentUserId, ct);

            if (person == null)
                throw new NotFoundException($"You are not a registered person in gym with ID {gymId}.");

            if (person.PersonType == PersonType.Owner)
                throw new InvalidOperationException("Owners cannot leave their own gym. Use Change Owner instead.");

            await _gymPersonRepo.DeleteAsync(person, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
