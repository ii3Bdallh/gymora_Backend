using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Service.Base;
using Domain.Events;


using Application.DTO.Model;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;

namespace Application.Service
{
    public class GymStaffService : BaseService<GymStaff, GymStaffRDTO, GymStaffCDTO, GymStaffUDTO>, IGymStaffService
    {
        private readonly IGymStaffRepo _gymStaffRepo;
        public GymStaffService(
            IGymStaffRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<GymStaffService> logger
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymStaffRepo = repo;
        }

        public Task<GymStaffRDTO> GetByGymIdAndUserIdAsync(int gymId, int userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<GymStaffRDTO> LinkAccountToGymAsync(int gymId, Guid inviteCode, CancellationToken ct = default)
        {

            var gymStaff = await _gymStaffRepo.LinkAccountToGymAsync(gymId, inviteCode, ct);

            if (gymStaff is null)
                throw new InvalidOperationException("Failed to link account to gym.");
            await _unitOfWork.SaveChangesAsync(ct);
            var result = _mapper.Map<GymStaffRDTO>(gymStaff);

            return result;
        }

        public async Task PaySalaryAsync(int staffId, DateTime? salaryValidFrom, DateTime? salaryValidUntil, CancellationToken ct = default)
        {
            var staff = await _repo.GetByIdAsync(staffId, isActive: true, trackChanges: true, cancellationToken: ct);
            if (staff == null)
            {
                throw new KeyNotFoundException($"GymStaff with ID {staffId} not found or is inactive.");
            }

            staff.SalaryValidFrom = salaryValidFrom;
            staff.SalaryValidUntil = salaryValidUntil;



            if (staff.Salary == null || staff.Salary <= 0)
            {
                throw new InvalidOperationException($"GymStaff with ID {staffId} does not have a valid salary configured.");
            }

            var now = DateTime.UtcNow;
            if (staff.SalaryValidFrom.HasValue && now < staff.SalaryValidFrom.Value)
            {
                throw new InvalidOperationException($"Salary for GymStaff {staffId} is not valid yet (Valid from: {staff.SalaryValidFrom.Value}).");
            }

            if (staff.SalaryValidUntil.HasValue && now > staff.SalaryValidUntil.Value)
            {
                throw new InvalidOperationException($"Salary for GymStaff {staffId} has expired (Expired on: {staff.SalaryValidUntil.Value}).");
            }

            await _repo.UpdateAsync(staff, ct);
            await base.PublishEntityChangedAsync(staffId, ct);

            // Publish SalaryPaidEvent
            await _publishEndpoint.Publish(new SalaryPaidEvent(
                staff.Id,
                staff.Salary.Value,
                now,
                staff.GymId
            ), ct);
            await _unitOfWork.SaveChangesAsync(ct);

        }
    }
}