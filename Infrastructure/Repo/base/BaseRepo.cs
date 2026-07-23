using System.Linq.Dynamic.Core;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Domain.Model.Base;
using Infrastructure.Cache;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.DTO;
using Domain.Interface;

namespace Infrastructure.Repo.Base
{
    public abstract class BaseRepo<T> : IBaseRepo<T> where T : class, IBaseEntity
    {
        protected readonly ApplicationDbContext context;
        protected readonly ILogger<BaseRepo<T>> logger;
        protected readonly QueryCache queryCache;

        protected BaseRepo(ApplicationDbContext context, ILogger<BaseRepo<T>> logger, QueryCache queryCache)
        {
            this.context = context;
            this.logger = logger;
            this.queryCache = queryCache;
        }

        #region Protected Methods
        protected virtual Func<IQueryable<T>, IQueryable<T>>? Includes() => null;

        protected virtual IQueryable<T> BuildQuery(bool isActive = true, bool trackChanges = false)
        {
            IQueryable<T> query = trackChanges ? DbSet : DbSet.AsNoTracking();
            query = query.Where(x => x.IsActive == isActive);

            // نقطة التوسّع: كل Repo مشتق يعمل Override ليها لو محتاج فلترة إضافية (Gym/Owned/etc)
            query = ApplyExtraFilters(query);

            return query;
        }

        protected virtual IQueryable<T> ApplyExtraFilters(IQueryable<T> query)
        {
            return query; // BaseRepo مش عارف حاجة عن اليوزر ولا الفلاتر الخاصة
        }
        #endregion

        public DbSet<T> DbSet => context.Set<T>();

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = BuildQuery(true);
            if (include != null) query = include(query);
            return await query.ToListAsync(cancellationToken);
        }

        public virtual IQueryable<T> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = BuildQuery(isActive, trackChanges);

            if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                query = query.Search(searchReq.SearchTerm, queryCache);

            if (searchReq.Filters is not null)
                query = query.ApplyFilters(searchReq.Filters, queryCache);

            var orderBy = !string.IsNullOrEmpty(searchReq.OrderBy) ? searchReq.OrderBy : "Id";
            var direction = searchReq.OrderDirection?.ToLower() == "desc" ? "descending" : "ascending";
            query = query.OrderBy($"{orderBy} {direction}");

            if (include != null) query = include(query);

            return query;
        }

        public virtual async Task<PaginatedRes<T>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            var countQuery = GetAllQuery(searchReq, isActive, trackChanges, cancellationToken);
            var totalCount = await countQuery.CountAsync(cancellationToken);

            var dataQuery = GetAllQuery(searchReq, isActive, trackChanges, cancellationToken);
            if (include != null) dataQuery = include(dataQuery);

            var pageItems = await dataQuery
                .Skip((searchReq.PageNumber - 1) * searchReq.PageSize)
                .Take(searchReq.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedRes<T>
            {
                PageNumber = searchReq.PageNumber,
                PageSize = searchReq.PageSize,
                TotalCount = totalCount,
                Items = pageItems
            };
        }

        public virtual async Task<T?> GetByIdAsync(
            int id, bool isActive = true, bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = BuildQuery(isActive, trackChanges);
            if (include != null) query = include(query);
            return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<T?> GetByIdIgnoringSecurityAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = trackChanges ? DbSet : DbSet.AsNoTracking();
            query = query.Where(x => x.IsActive == isActive);
            if (include != null) query = include(query);
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public virtual Task<T> AddAsync(T item, CancellationToken cancellationToken = default)
        {
            DbSet.Add(item);
            return Task.FromResult(item);
        }

        public virtual Task<T> UpdateAsync(T item, CancellationToken cancellationToken = default)
        {
            DbSet.Update(item);
            return Task.FromResult(item);
        }

        public virtual Task<T> DeleteAsync(T item, CancellationToken cancellationToken = default)
        {
            item.IsActive = false;
            DbSet.Update(item);
            return Task.FromResult(item);
        }

        public virtual Task<T> HardDeleteAsync(T item, CancellationToken cancellationToken = default)
        {
            DbSet.Remove(item);
            return Task.FromResult(item);
        }
    }
}