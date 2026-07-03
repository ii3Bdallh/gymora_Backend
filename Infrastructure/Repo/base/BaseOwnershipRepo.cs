using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo.Base;
using Domain.Model.Auth;
using Domain.Model.Base;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repo.Base
{
    public abstract class BaseOwnershipRepo<T> :
    BaseRepo<T>,
    IOwnershipRepo<T>
      where T : AuditableEntity
    {
        protected readonly CurrentUser currentUser;

        protected BaseOwnershipRepo(
            ApplicationDbContext context,
            ILogger logger,
            CurrentUser currentUser,
            QueryCache queryCache)
            : base(context, logger, queryCache)
        {
            this.currentUser = currentUser;
        }

        /// <summary>
        /// Query مخصص للـ WRITE فقط (Update / Delete)
        /// </summary>
        protected virtual IQueryable<T> OwnedBaseQuery()
        {
            return currentUser.IsAdmin
                ? DbSet
                : DbSet.Where(x => x.CreatedById == currentUser.UserId);
        }


        /// <summary>
        /// Override GetAllQuery to support optional CreatedById filtering
        /// </summary>
        public override IQueryable<T> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var query = base.GetAllQuery(searchReq, IsActive, trackChanges, cancellationToken);

            if (searchReq.CreatedById.HasValue)
                query = query.Where(x => x.CreatedById == searchReq.CreatedById.Value);

            return query;
        }

        /// <summary>
        /// UPDATE → Ownership enforced
        /// </summary>
        public override async Task<T> UpdateAsync(T item, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            if (!currentUser.IsAdmin && item.CreatedById != currentUser.UserId)
                throw new ForbiddenException("item not found");

            return await base.UpdateAsync(item, trackChanges, cancellationToken);
        }

        /// <summary>
        /// DELETE → Ownership enforced, soft delete
        /// </summary>
        public override async Task<T> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await OwnedBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (item is null)
                throw new ForbiddenException("item not found");

            item.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);

            context.Entry(item).State = EntityState.Detached;

            return item;
        }

        public async Task<IEnumerable<T?>> GetAllOwnedAsync(
         CancellationToken cancellationToken = default)
        {
            return await OwnedBaseQuery()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public IQueryable<T> OwnedQuery(
            PaginatedSearchReq searchReq,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = OwnedBaseQuery();

            if (IsActive)
                query = query.Where(x => x.IsActive);
            else
                query = query.Where(x => !x.IsActive);

            if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                query = query.Search(searchReq.SearchTerm, queryCache);

     

            if (searchReq.Filters is not null)
                query = query.ApplyFilters(searchReq.Filters, queryCache);

            return trackChanges ? query : query.AsNoTracking();
        }

        public async Task<PaginatedRes<T>> GetPageOwnedAsync(
            PaginatedSearchReq searchReq,
            bool IsActive = true,
            bool trackChanges = false,
             CancellationToken cancellationToken = default)
        {
            var query = OwnedQuery(searchReq, IsActive, trackChanges, cancellationToken);

            var items = await query
                .Skip((searchReq.PageNumber - 1) * searchReq.PageSize)
                .Take(searchReq.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedRes<T>
            {
                PageNumber = searchReq.PageNumber,
                PageSize = searchReq.PageSize,
                TotalCount = await query.CountAsync(cancellationToken),
                Items = items
            };
        }

        public async Task<T?> GetByIdOwnedAsync(
            int id,
            bool IsActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var item = await OwnedBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == IsActive, cancellationToken);

            if (item is null)
                throw new ForbiddenException("item not found");

            if (!trackChanges)
                context.Entry(item).State = EntityState.Detached;

            return item;
        }

       
    }

}