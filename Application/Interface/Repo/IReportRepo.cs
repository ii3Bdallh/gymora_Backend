using Domain.Model;
using System.Linq;

namespace Application.Interface.Repo
{
    public interface IReportRepo
    {
        IQueryable<Revenue> GetRevenuesQuery();
        IQueryable<Expense> GetExpensesQuery();
        IQueryable<Attendance> GetAttendancesQuery();
        IQueryable<GymPerson> GetGymPersonsQuery();
    }
}
