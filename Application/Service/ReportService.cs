using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using Domain.Enum;
using Domain.Model;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class ReportService : IReportService
    {
        protected readonly IReportRepo ReportRepo;
        protected readonly CurrentUser CurrentUser;
        protected readonly IMapper Mapper;

        public ReportService(
            IReportRepo reportRepo,
            CurrentUser currentUser,
            IMapper mapper)
        {
            ReportRepo = reportRepo;
            CurrentUser = currentUser;
            Mapper = mapper;
        }

        public async Task<RevenueReportRDTO> GetRevenueReportAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            ValidateAccess(gymId);

            var allRevenuesQuery = ReportRepo.GetRevenuesQuery();
            var currentRevenues = await allRevenuesQuery
                .Where(r => r.RevenueDate >= fromDate && r.RevenueDate <= toDate)
                .Include(r => r.GymMember)
                .ToListAsync(ct);

            var currentTotal = currentRevenues.Sum(r => r.Amount);

            var days = (toDate - fromDate).Days;
            var prevFromDate = fromDate.AddDays(-days - 1);
            var prevToDate = fromDate.AddDays(-1);

            var prevTotal = await allRevenuesQuery
                .Where(r => r.RevenueDate >= prevFromDate && r.RevenueDate <= prevToDate)
                .SumAsync(r => r.Amount, ct);

            decimal percentageChange = 0;
            if (prevTotal > 0)
            {
                percentageChange = ((currentTotal - prevTotal) / prevTotal) * 100;
            }
            else if (currentTotal > 0)
            {
                percentageChange = 100;
            }

            var chartPoints = currentRevenues
                .GroupBy(r => r.RevenueDate.Date)
                .Select(g => new FinancialChartPointRDTO
                {
                    Date = g.Key,
                    Amount = g.Sum(r => r.Amount)
                })
                .ToList();

            var chartData = new List<FinancialChartPointRDTO>();
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var point = chartPoints.FirstOrDefault(p => p.Date == date);
                chartData.Add(new FinancialChartPointRDTO
                {
                    Date = date,
                    Amount = point?.Amount ?? 0
                });
            }

            var recentRevenues = currentRevenues
                .OrderByDescending(r => r.RevenueDate)
                .Take(10)
                .Select(r => Mapper.Map<RevenueRDTO>(r))
                .ToList();

            return new RevenueReportRDTO
            {
                TotalRevenue = currentTotal,
                PercentageChangeVsLastPeriod = percentageChange,
                ChartDataPoints = chartData,
                RecentTransactions = recentRevenues
            };
        }

        public async Task<ExpenseReportRDTO> GetExpenseReportAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            ValidateAccess(gymId);

            var allExpensesQuery = ReportRepo.GetExpensesQuery();
            var currentExpenses = await allExpensesQuery
                .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
                .Include(e => e.GymStaff)
                .ToListAsync(ct);

            var currentTotal = currentExpenses.Sum(e => e.Amount);

            var days = (toDate - fromDate).Days;
            var prevFromDate = fromDate.AddDays(-days - 1);
            var prevToDate = fromDate.AddDays(-1);

            var prevTotal = await allExpensesQuery
                .Where(e => e.ExpenseDate >= prevFromDate && e.ExpenseDate <= prevToDate)
                .SumAsync(e => e.Amount, ct);

            decimal percentageChange = 0;
            if (prevTotal > 0)
            {
                percentageChange = ((currentTotal - prevTotal) / prevTotal) * 100;
            }
            else if (currentTotal > 0)
            {
                percentageChange = 100;
            }

            var chartPoints = currentExpenses
                .GroupBy(e => e.ExpenseDate.Date)
                .Select(g => new FinancialChartPointRDTO
                {
                    Date = g.Key,
                    Amount = g.Sum(e => e.Amount)
                })
                .ToList();

            var chartData = new List<FinancialChartPointRDTO>();
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var point = chartPoints.FirstOrDefault(p => p.Date == date);
                chartData.Add(new FinancialChartPointRDTO
                {
                    Date = date,
                    Amount = point?.Amount ?? 0
                });
            }

            var recentExpenses = currentExpenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(10)
                .Select(e => Mapper.Map<ExpenseRDTO>(e))
                .ToList();

            return new ExpenseReportRDTO
            {
                TotalExpense = currentTotal,
                PercentageChangeVsLastPeriod = percentageChange,
                ChartDataPoints = chartData,
                RecentTransactions = recentExpenses
            };
        }

        public async Task<AttendanceReportRDTO> GetAttendanceReportAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            ValidateAccess(gymId);

            var allAttendancesQuery = ReportRepo.GetAttendancesQuery();
            var currentAttendances = await allAttendancesQuery
                .Where(a => a.GymId == gymId && a.CheckInTime >= fromDate && a.CheckInTime <= toDate)
                .Include(a => a.Member)
                    .ThenInclude(m => m.MemberProfile)
                .Include(a => a.RecordedBy)
                .ToListAsync(ct);

            var currentTotal = currentAttendances.Count;

            var days = (toDate - fromDate).Days;
            var prevFromDate = fromDate.AddDays(-days - 1);
            var prevToDate = fromDate.AddDays(-1);

            var prevTotal = await allAttendancesQuery
                .Where(a => a.GymId == gymId && a.CheckInTime >= prevFromDate && a.CheckInTime <= prevToDate)
                .CountAsync(ct);

            decimal percentageChange = 0;
            if (prevTotal > 0)
            {
                percentageChange = (((decimal)currentTotal - prevTotal) / prevTotal) * 100;
            }
            else if (currentTotal > 0)
            {
                percentageChange = 100;
            }

            var chartPoints = currentAttendances
                .GroupBy(a => a.CheckInTime.Date)
                .Select(g => new FinancialChartPointRDTO
                {
                    Date = g.Key,
                    Amount = g.Count()
                })
                .ToList();

            var chartData = new List<FinancialChartPointRDTO>();
            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var point = chartPoints.FirstOrDefault(p => p.Date == date);
                chartData.Add(new FinancialChartPointRDTO
                {
                    Date = date,
                    Amount = point?.Amount ?? 0
                });
            }

            var recentTransactions = currentAttendances
                .OrderByDescending(a => a.CheckInTime)
                .Take(10)
                .Select(a => Mapper.Map<AttendanceLogItemRDTO>(a))
                .ToList();

            // Stats calculations
            var today = DateTime.UtcNow.Date;
            var todayCheckIns = await allAttendancesQuery
                .Where(a => a.GymId == gymId && a.CheckInTime >= today)
                .CountAsync(ct);

            var timeLimit = DateTime.UtcNow.AddHours(-4);
            var currentlyInside = await allAttendancesQuery
                .Where(a => a.GymId == gymId && a.CheckInTime >= timeLimit)
                .CountAsync(ct);

            var personsQuery = ReportRepo.GetGymPersonsQuery().Where(p => p.GymId == gymId && p.PersonType == PersonType.Member);
            var activeMembers = await personsQuery
                .Where(p => p.AccessStatus == GymPersonAccessStatus.Active && p.MemberProfile != null && p.MemberProfile.MembershipEndDate.HasValue && p.MemberProfile.MembershipEndDate.Value > DateTime.UtcNow)
                .CountAsync(ct);

            var expiredMembers = await personsQuery
                .Where(p => p.AccessStatus == GymPersonAccessStatus.Active && (p.MemberProfile == null || !p.MemberProfile.MembershipEndDate.HasValue || p.MemberProfile.MembershipEndDate.Value <= DateTime.UtcNow))
                .CountAsync(ct);

            var stats = new AttendanceDashboardStatsRDTO(todayCheckIns, currentlyInside, activeMembers, expiredMembers);

            return new AttendanceReportRDTO
            {
                TotalCheckIns = currentTotal,
                PercentageChangeVsLastPeriod = percentageChange,
                ChartDataPoints = chartData,
                RecentTransactions = recentTransactions,
                Stats = stats
            };
        }

        public async Task<GymAttendanceDashboardRDTO> GetDashboardAsync(int gymId, CancellationToken ct = default)
        {
            var report = await GetAttendanceReportAsync(gymId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, ct);
            var recentEntries = report.RecentTransactions.Select(x => new RecentCheckInItemRDTO
            {
                AttendanceId = x.Id,
                MemberId = x.MemberId,
                MemberFullName = x.MemberFullName,
                ProfilePictureUrl = null,
                PlanType = x.MembershipStatus,
                CheckInTime = x.CheckInTime
            }).ToList();

            return new GymAttendanceDashboardRDTO
            {
                GymId = gymId,
                Stats = report.Stats ?? new AttendanceDashboardStatsRDTO(0, 0, 0, 0),
                RecentEntries = recentEntries
            };
        }

        private void ValidateAccess(int gymId)
        {
            if (gymId != (CurrentUser.CurrentGymId ?? 0) && !CurrentUser.IsSuperAdmin)
            {
                throw new ForbiddenException("You are not authorized to view reports for this gym.");
            }
        }
    }
}
