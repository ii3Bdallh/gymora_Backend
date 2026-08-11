using Domain.Model;
using System.Linq;

namespace Application.Interface.Repo
{
    public interface IFinancialReportRepo
    {
        IQueryable<Revenue> GetRevenuesQuery();
        IQueryable<Expense> GetExpensesQuery();
        IQueryable<Attendance> GetAttendancesQuery();
    }
}
