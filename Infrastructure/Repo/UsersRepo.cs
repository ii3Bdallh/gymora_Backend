using Application.Interface.Repo;
using Domain.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Repo.Base;
using Domain.Model.Auth;
using Google;
using Infrastructure.Persistence;
using Infrastructure.Cache;
using Application.Model;
using Microsoft.EntityFrameworkCore;
using Application.DTO.Pagintion;
using Infrastructure.Extensions;
using System.Linq.Dynamic.Core;

namespace Infrastructure.Repo
{
    public class UsersRepo(ApplicationDbContext context, QueryCache queryCache)
    : IUsersRepo
    {
        protected virtual Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? Includes()
        {
            return null;
        }
        public DbSet<ApplicationUser> DbSet => context.Set<ApplicationUser>();




        protected virtual IQueryable<ApplicationUser> BuildQuery(
bool isActive = true,
bool trackChanges = false)
        {
            IQueryable<ApplicationUser> query = trackChanges
                ? DbSet
                : DbSet.AsNoTracking();

            // query = query.Where(x => x.IsActive == isActive);


            return query;
        }

        public virtual async Task<IEnumerable<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default, Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null)
        {
            IQueryable<ApplicationUser> query = BuildQuery(true);

            if (include != null)
                query = include(query);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual IQueryable<ApplicationUser> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null)
        {
            IQueryable<ApplicationUser> query = BuildQuery(isActive, trackChanges);

            // تطبيق الـ Extensions الاحترافية بتاعتك
            if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                query = query.Search(searchReq.SearchTerm, queryCache);

            if (searchReq.Filters is not null)
                query = query.ApplyFilters(searchReq.Filters, queryCache);

            // الترتيب الديناميكي
            var orderBy = !string.IsNullOrEmpty(searchReq.OrderBy) ? searchReq.OrderBy : "Id";
            var direction = searchReq.OrderDirection?.ToLower() == "desc" ? "descending" : "ascending";
            query = query.OrderBy($"{orderBy} {direction}");

            // دمج الـ Includes لو وُجدت
            if (include != null)
            {
                query = include(query);
            }

            return query;
        }

        public virtual async Task<PaginatedRes<ApplicationUser>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
                 Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null)
        {
            // 1. بنجيب كويري "خفيفة" من غير Includes عشان نعمل عليها الـ Count بسرعة
            var countQuery = GetAllQuery(searchReq, isActive, trackChanges, cancellationToken);
            var totalCount = await countQuery.CountAsync(cancellationToken);

            // 2. بنجيب الكويري الكاملة بالـ Includes عشان نسحب الداتا مع الـ Children بتوعها
            var dataQuery = GetAllQuery(searchReq, isActive, trackChanges, cancellationToken);
            if (include != null)
                dataQuery = include(dataQuery);

            var pageItems = await dataQuery
                .Skip((searchReq.PageNumber - 1) * searchReq.PageSize)
                .Take(searchReq.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedRes<ApplicationUser>
            {
                PageNumber = searchReq.PageNumber,
                PageSize = searchReq.PageSize,
                TotalCount = totalCount,
                Items = pageItems
            };
        }

        public virtual async Task<ApplicationUser?> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null)
        {
            IQueryable<ApplicationUser> query = BuildQuery(isActive, trackChanges);

            if (include != null)
                query = include(query);

            return await query
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<ApplicationUser?> GetByIdIgnoringSecurityAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default, Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>? include = null)
        {
            IQueryable<ApplicationUser> query = trackChanges
       ? DbSet
       : DbSet.AsNoTracking();


            if (include != null)
                query = include(query);

            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}