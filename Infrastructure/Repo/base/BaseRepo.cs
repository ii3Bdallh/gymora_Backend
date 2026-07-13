using System.Linq.Expressions;
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

namespace Infrastructure.Repo.Base
{
    public abstract class BaseRepo<T> : IBaseRepo<T> where T : BaseEntity
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

        public DbSet<T> DbSet => context.Set<T>();

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            if (include != null)
                return await include(DbSet.AsNoTracking()).ToListAsync(cancellationToken);
            return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
        }
        public virtual IQueryable<T> GetQueryable(bool trackChanges = false)
        {
            return trackChanges ? DbSet : DbSet.AsNoTracking();
        }
        public virtual IQueryable<T> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            // ضبط الـ Tracking من بداية الـ Pipeline لتوفير الـ CPU والـ Memory
            IQueryable<T> query = trackChanges ? DbSet : DbSet.AsNoTracking();

            query = isActive ? query.Where(x => x.IsActive) : query.Where(x => !x.IsActive);

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

        public virtual async Task<PaginatedRes<T>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
                 Func<IQueryable<T>, IQueryable<T>>? include = null)
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

            return new PaginatedRes<T>
            {
                PageNumber = searchReq.PageNumber,
                PageSize = searchReq.PageSize,
                TotalCount = totalCount,
                Items = pageItems
            };
        }

        public virtual async Task<T?> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = trackChanges ? DbSet : DbSet.AsNoTracking();

            if (include != null)
                query = include(query);


            return await query
                .Where(x => x.Id == id && x.IsActive == isActive)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public virtual Task<T> AddAsync(T item, CancellationToken cancellationToken = default)
        {
            // العملية تتم في الـ Memory فقط
            DbSet.Add(item);
            return Task.FromResult(item);
        }

        public virtual Task<T> UpdateAsync(T item, CancellationToken cancellationToken = default)
        {
            // العملية تتم في الـ Memory فقط
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