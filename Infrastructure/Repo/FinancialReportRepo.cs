using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repo
{
    public class FinancialReportRepo : IFinancialReportRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrentUser _currentUser;

        public FinancialReportRepo(ApplicationDbContext context, CurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public IQueryable<Revenue> GetRevenuesQuery()
        {
            IQueryable<Revenue> query = _context.Revenues.AsNoTracking();
            if (!_currentUser.IsSuperAdmin && _currentUser.CurrentGymId.HasValue)
            {
                query = query.Where(r => r.GymId == _currentUser.CurrentGymId.Value);
            }
            return query;
        }

        public IQueryable<Expense> GetExpensesQuery()
        {
            IQueryable<Expense> query = _context.Expenses.AsNoTracking();
            if (!_currentUser.IsSuperAdmin && _currentUser.CurrentGymId.HasValue)
            {
                query = query.Where(e => e.GymId == _currentUser.CurrentGymId.Value);
            }
            return query;
        }

        public IQueryable<Attendance> GetAttendancesQuery()
        {
            IQueryable<Attendance> query = _context.Attendances.AsNoTracking();
            if (!_currentUser.IsSuperAdmin && _currentUser.CurrentGymId.HasValue)
            {
                query = query.Where(a => a.GymId == _currentUser.CurrentGymId.Value);
            }
            return query;
        }
    }
}
