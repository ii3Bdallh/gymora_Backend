using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repo
{
    public class ReportRepo : IReportRepo
    {
        protected readonly ApplicationDbContext Context;
        protected readonly CurrentUser CurrentUser;

        public ReportRepo(ApplicationDbContext context, CurrentUser currentUser)
        {
            Context = context;
            CurrentUser = currentUser;
        }

        public IQueryable<Revenue> GetRevenuesQuery()
        {
            IQueryable<Revenue> query = Context.Revenues.AsNoTracking();
            if (!CurrentUser.IsSuperAdmin && CurrentUser.CurrentGymId.HasValue)
            {
                query = query.Where(r => r.GymId == CurrentUser.CurrentGymId.Value);
            }
            return query;
        }

        public IQueryable<Expense> GetExpensesQuery()
        {
            IQueryable<Expense> query = Context.Expenses.AsNoTracking();
            if (!CurrentUser.IsSuperAdmin && CurrentUser.CurrentGymId.HasValue)
            {
                query = query.Where(e => e.GymId == CurrentUser.CurrentGymId.Value);
            }
            return query;
        }

        public IQueryable<Attendance> GetAttendancesQuery()
        {
            IQueryable<Attendance> query = Context.Attendances.AsNoTracking();
            if (!CurrentUser.IsSuperAdmin && CurrentUser.CurrentGymId.HasValue)
            {
                query = query.Where(a => a.GymId == CurrentUser.CurrentGymId.Value);
            }
            return query;
        }

        public IQueryable<GymPerson> GetGymPersonsQuery()
        {
            IQueryable<GymPerson> query = Context.GymPerson.AsNoTracking();
            if (!CurrentUser.IsSuperAdmin && CurrentUser.CurrentGymId.HasValue)
            {
                query = query.Where(p => p.GymId == CurrentUser.CurrentGymId.Value);
            }
            return query;
        }
    }
}
