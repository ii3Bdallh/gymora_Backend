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
    public class ExpenseRepo(ApplicationDbContext context, ILogger<ExpenseRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseGymRepo<Expense>(context, logger, queryCache, currentUser), IExpenseRepo
    {
        protected override Func<IQueryable<Expense>, IQueryable<Expense>>? Includes()
        {
            return query => query.Include(x => x.GymStaff);
        }

        public override  Task<PaginatedRes<Expense>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Expense>, IQueryable<Expense>>? include = null)
        {
            include ??= Includes();
            return  base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }
        public override  Task<Expense?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<Expense>, IQueryable<Expense>>? include = null)
        {
            include ??= Includes();
            return  base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }
    }
}
