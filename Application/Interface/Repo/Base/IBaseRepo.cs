using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Enum;
using Domain.Model.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IBaseRepo<T> where T : BaseEntity
    {
        public DbSet<T> DbSet { get; }
        public Task<IEnumerable<T?>> GetAllAsync(CancellationToken cancellationToken = default);
        public IQueryable<T> GetAllQuery(PaginatedSearchReq searchReq, bool IsActive = true, bool trackChanges = false, CancellationToken cancellationToken = default);
        public Task<PaginatedRes<T>> GetPageAsync(PaginatedSearchReq searchReq, bool IsActive = true, bool trackChanges = false, CancellationToken cancellationToken = default);
        public Task<T> GetByIdAsync(int id, bool IsActive = true, bool trackChanges = false, CancellationToken cancellationToken = default);
        public Task<T> UpdateAsync(T item, bool trackChanges = false, CancellationToken cancellationToken = default);
        public Task<T> AddAsync(T item, bool trackChanges = false, CancellationToken cancellationToken = default);
        public Task<T> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}

