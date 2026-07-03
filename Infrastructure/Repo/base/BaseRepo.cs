using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Domain.Model.Base;
using Infrastructure.Cache;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace Infrastructure.Repo.Base
{
    public abstract class BaseRepo<T> : IBaseRepo<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext context;
        protected readonly ILogger logger;
        protected readonly QueryCache queryCache;

        public BaseRepo(ApplicationDbContext context, ILogger logger, QueryCache queryCache)
        {
            this.context = context;
            this.logger = logger;
            this.queryCache = queryCache;
        }

        public DbSet<T> DbSet => context.Set<T>();

        public async Task<IEnumerable<T?>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public virtual IQueryable<T> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = DbSet;

            query = IsActive
                ? query.Where(x => x.IsActive)
                : query.Where(x => !x.IsActive);

            if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                query = query.Search(searchReq.SearchTerm, queryCache);

            if (searchReq.Filters is not null)
                query = query.ApplyFilters(searchReq.Filters, queryCache);

            var orderBy = !string.IsNullOrEmpty(searchReq.OrderBy) ? searchReq.OrderBy : "Id";
            var direction = searchReq.OrderDirection?.ToLower() == "desc" ? "descending" : "ascending";
            query = query.OrderBy($"{orderBy} {direction}");

            return trackChanges ? query : query.AsNoTracking();
        }

        public virtual async Task<PaginatedRes<T>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var query = GetAllQuery(searchReq, IsActive, trackChanges, cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);

            var pageItems = await query
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

        public virtual async Task<T> GetByIdAsync(
            int id,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var item = await DbSet
                .Where(x => x.Id == id && x.IsActive == IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
                throw new ForbiddenException("You don't have access to this resource or it doesn't exist.");

            if (!trackChanges)
                context.Entry(item).State = EntityState.Detached;

            return item;
        }

        public virtual async Task<T> UpdateAsync(
            T item,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                DbSet.Update(item);
                await context.SaveChangesAsync(cancellationToken);

                if (!trackChanges)
                    context.Entry(item).State = EntityState.Detached;

                return item;
            }
            catch (Exception ex)
            {
                HandleDatabaseException(ex, "updating");
                throw;
            }
        }

        public virtual async Task<T> AddAsync(
            T item,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await DbSet.AddAsync(item, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                if (!trackChanges)
                    context.Entry(item).State = EntityState.Detached;

                return item;
            }
            catch (Exception ex)
            {
                HandleDatabaseException(ex, "adding");
                throw;
            }
        }

        public virtual async Task<T> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await DbSet.FindAsync(id, cancellationToken);
            if (item is null)
                throw new ForbiddenException("You don't have access to this resource or it doesn't exist.");

            item.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);

            context.Entry(item).State = EntityState.Detached;

            return item;
        }

        protected virtual async Task<T> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await DbSet.FindAsync(id, cancellationToken);
            if (item is null)
                throw new ForbiddenException("You don't have access to this resource or it doesn't exist.");

            DbSet.Remove(item);
            await context.SaveChangesAsync(cancellationToken);

            return item;
        }

        private void HandleDatabaseException(Exception ex, string operationType = "database operation")
        {
            if (ex is DbUpdateException dbEx)
            {
                if (dbEx.InnerException != null && dbEx.InnerException.Message.ToLower().Contains("unique"))
                {
                    logger.LogWarning(dbEx, "Unique constraint violation when {operationType} entity of type {EntityType}", operationType, typeof(T).Name);
                    throw new BadRequestException("This value already exists and must be unique.");
                }

                if (dbEx.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                {
                    logger.LogWarning(sqlEx, "Duplicate key error when {operationType} entity of type {EntityType}", operationType, typeof(T).Name);
                    throw new BadRequestException("This value already exists. Please use a different one.");
                }

                logger.LogError(dbEx, "Database update error while {operationType} entity of type {EntityType}", operationType, typeof(T).Name);
                throw new BadRequestException("A database error occurred while saving the record.");
            }
            else
            {
                logger.LogError(ex, "Unexpected error while {operationType} entity of type {EntityType}", operationType, typeof(T).Name);
                throw new BadRequestException("An unexpected error occurred. Please try again later.");
            }
        }
    }
}
