using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Enum;
using Domain.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Service.Base;

namespace Application.Service
{
    public class AttendanceService
        : BaseGymService<Attendance, AttendanceLogItemRDTO, RecordCheckInCDTO, RecordCheckInUDTO>, IAttendanceService
    {
        private readonly IGymPersonRepo _gymPersonRepo;
        private readonly IAttendanceRepo _attendanceRepo;

        public AttendanceService(
            IAttendanceRepo repo,
            IGymPersonRepo gymPersonRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<AttendanceService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymPersonRepo = gymPersonRepo;
            _attendanceRepo = repo;
        }

        public async Task<GymAttendanceDashboardRDTO> GetDashboardAsync(int gymId, CancellationToken ct)
        {
            if (CurrentGymId != gymId)
                throw new ForbiddenException("You are not authorized to access this gym.");

            // 1. Calculate stats via repository calls
            var todayCheckIns = await _attendanceRepo.GetTodayCheckInsCountAsync(gymId, ct);
            var currentlyInside = await _attendanceRepo.GetCurrentlyInsideCountAsync(gymId, ct);
            var activeMembers = await _gymPersonRepo.GetActiveMembersCountAsync(gymId, ct);
            var expiredMembers = await _gymPersonRepo.GetExpiredMembersCountAsync(gymId, ct);

            // 2. Fetch last 10 entries
            var recentEntriesList = await _attendanceRepo.GetRecentEntriesAsync(gymId, 10, ct);

            var recentEntries = recentEntriesList.Select(x => new RecentCheckInItemRDTO
            {
                AttendanceId = x.Id,
                MemberId = x.MemberId,
                MemberFullName = x.Member.Name,
                ProfilePictureUrl = x.Member.PhotoUrl,
                PlanType = x.Member.MemberProfile?.PlanName ?? "Basic",
                CheckInTime = x.CheckInTime
            }).ToList();

            return new GymAttendanceDashboardRDTO
            {
                GymId = gymId,
                Stats = new AttendanceDashboardStatsRDTO(todayCheckIns, currentlyInside, activeMembers, expiredMembers),
                RecentEntries = recentEntries
            };
        }

        protected override async Task BeforeAddAsync(RecordCheckInCDTO dto, CancellationToken ct)
        {
            await base.BeforeAddAsync(dto, ct);

            var member = await _gymPersonRepo.GetByIdAsync(dto.MemberId, false, ct);
            if (member == null || member.PersonType != PersonType.Member || member.GymId != dto.GymId || member.AccessStatus != GymPersonAccessStatus.Active)
                throw new NotFoundException($"Member with ID {dto.MemberId} was not found.");

            if (member.MemberProfile == null || !member.MemberProfile.MembershipEndDate.HasValue || member.MemberProfile.MembershipEndDate.Value < DateTime.UtcNow)
            {
                throw new UnprocessableEntityException("MEMBERSHIP_INACTIVE_OR_EXPIRED", "Membership is inactive or expired.");
            }
        }






    }
}
