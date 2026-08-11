using System;
using System.Collections.Generic;

namespace Application.DTO.Model
{
    public record FinancialChartPointRDTO
    {
        public DateTime Date { get; init; }
        public decimal Amount { get; init; }
    }

    public record RevenueReportRDTO
    {
        public decimal TotalRevenue { get; init; }
        public decimal PercentageChangeVsLastPeriod { get; init; }
        public List<FinancialChartPointRDTO> ChartDataPoints { get; init; } = new();
        public List<RevenueRDTO> RecentTransactions { get; init; } = new();
    }

    public record ExpenseReportRDTO
    {
        public decimal TotalExpense { get; init; }
        public decimal PercentageChangeVsLastPeriod { get; init; }
        public List<FinancialChartPointRDTO> ChartDataPoints { get; init; } = new();
        public List<ExpenseRDTO> RecentTransactions { get; init; } = new();
    }
}
