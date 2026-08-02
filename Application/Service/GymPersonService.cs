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
    public class GymPersonService : BaseService<GymPerson, GymPersonRDTO, GymPersonCDTO, GymPersonUDTO>, IGymPersonService
    {
        private readonly IGymPersonRepo _gymPersonRepo;

        public GymPersonService(
            IGymPersonRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<GymPersonService> logger
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymPersonRepo = repo;
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
            if (entity.PersonType == PersonType.Staff || entity.PersonType == PersonType.Both)
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
            if (entity.PersonType == PersonType.Member || entity.PersonType == PersonType.Both)
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
            var person = await _repo.GetByIdAsync(staffId, isActive: true, trackChanges: true, cancellationToken: ct);
            if (person == null)
            {
                throw new KeyNotFoundException($"GymPerson with ID {staffId} not found or is inactive.");
            }

            if (person.PersonType != PersonType.Staff && person.PersonType != PersonType.Both)
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






    }
}
