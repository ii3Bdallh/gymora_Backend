using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class AttendanceRepo(ApplicationDbContext context, ILogger<AttendanceRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<Attendance>(context, logger, queryCache, currentUser), IAttendanceRepo
    {
        protected override Func<IQueryable<Attendance>, IQueryable<Attendance>>? Includes()
        {
            return query => query
                .Include(x => x.Member)
                    .ThenInclude(m => m.MemberProfile);
            // .Include(x => x.RecordedBy);
        }

        public override IQueryable<Attendance> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Attendance>, IQueryable<Attendance>>? include = null)
        {
            var query = base.GetAllQuery(searchReq, trackChanges, cancellationToken, include);

            if (searchReq is MemberAttendanceHistoryPagedReq memberReq)
            {
                query = query.Where(x => x.MemberId == memberReq.MemberId);
            }

            return query.OrderByDescending(x => x.CheckInTime);
        }

        public override async Task<PaginatedRes<Attendance>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Attendance>, IQueryable<Attendance>>? include = null)
        {
            include ??= Includes();
            return await base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }


        public override async Task<Attendance?> GetByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default, Func<IQueryable<Attendance>, IQueryable<Attendance>>? include = null)
        {
            include ??= Includes();
            return await base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }

        public async Task<int> GetTodayCheckInsCountAsync(int gymId, CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;
            return await DbSet
                .Where(x => x.GymId == gymId && x.CheckInTime >= today)
                .CountAsync(ct);
        }

        public async Task<int> GetCurrentlyInsideCountAsync(int gymId, CancellationToken ct = default)
        {
            var timeLimit = DateTime.UtcNow.AddHours(-4);
            return await DbSet
                .Where(x => x.GymId == gymId && x.CheckInTime >= timeLimit)
                .CountAsync(ct);
        }

        public async Task<List<Attendance>> GetRecentEntriesAsync(int gymId, int count, CancellationToken ct = default)
        {
            return await DbSet
                .Include(x => x.Member)
                    .ThenInclude(m => m.MemberProfile)
                .Where(x => x.GymId == gymId)
                .OrderByDescending(x => x.CheckInTime)
                .Take(count)
                .ToListAsync(ct);
        }

        // public async Task<List<Attendance>> GetAttendanceReportRecordsAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        // {
        //     return await DbSet
        //         .Include(x => x.Member)
        //         .Where(x => x.GymId == gymId && x.CheckInTime >= fromDate && x.CheckInTime <= toDate)
        //         .ToListAsync(ct);
        // }
    }
}
