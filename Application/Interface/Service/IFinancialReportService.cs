using Application.DTO.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IFinancialReportService
    {
        Task<RevenueReportRDTO> GetRevenueReportAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
        Task<ExpenseReportRDTO> GetExpenseReportAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    }
}
