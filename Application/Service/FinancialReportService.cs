using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly IFinancialReportRepo _reportRepo;
        private readonly CurrentUser _currentUser;
        private readonly IMapper _mapper;

        public FinancialReportService(
            IFinancialReportRepo reportRepo,
            CurrentUser currentUser,
            IMapper mapper)
        {
            _reportRepo = reportRepo;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<RevenueReportRDTO> GetRevenueReportAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            ValidateAccess(gymId);

            var allRevenuesQuery = _reportRepo.GetRevenuesQuery();
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
                .Select(r => _mapper.Map<RevenueRDTO>(r))
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

            var allExpensesQuery = _reportRepo.GetExpensesQuery();
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
                .Select(e => _mapper.Map<ExpenseRDTO>(e))
                .ToList();

            return new ExpenseReportRDTO
            {
                TotalExpense = currentTotal,
                PercentageChangeVsLastPeriod = percentageChange,
                ChartDataPoints = chartData,
                RecentTransactions = recentExpenses
            };
        }

        private void ValidateAccess(int gymId)
        {
            if (gymId != (_currentUser.CurrentGymId ?? 0) && !_currentUser.IsSuperAdmin)
            {
                throw new ForbiddenException("You are not authorized to view reports for this gym.");
            }
        }
    }
}
