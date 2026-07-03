using Domain.Model;
using Domain.Enum;
using Application.DTO;
using Application.DTO.Pagintion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo.Entity
{
    /// <summary>
    /// User repository interface for managing AppUser entities and role assignments.
    /// Provides methods for user management, role assignment, and role-based queries.
    /// </summary>
    public interface IUserRepo
    {
        #region Read Operations
        /// <summary>
        /// Get all users without pagination
        /// </summary>
        Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a query for users that can be further filtered
        /// </summary>
        IQueryable<AppUser> GetAllQuery(PaginatedSearchReq searchReq, bool trackChanges = false);

        /// <summary>
        /// Get paginated list of users with search and sorting
        /// </summary>
        Task<PaginatedRes<AppUser>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a single user by ID
        /// </summary>
        Task<AppUser?> GetByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all users with a specific role
        /// </summary>
        Task<IEnumerable<AppUser>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated list of users with a specific role
        /// </summary>
        Task<PaginatedRes<AppUser>> GetByRolePagedAsync(
            string roleName,
            PaginatedSearchReq searchReq,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user has a specific role
        /// </summary>
        Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all roles for a specific user
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by email
        /// </summary>
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        #endregion

        #region Write Operations
        /// <summary>
        /// Assign a role to a user
        /// </summary>
        Task AssignRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        Task RemoveRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Assign multiple roles to a user
        /// </summary>
        Task AssignRolesAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update user
        /// </summary>
        Task<AppUser> UpdateAsync(AppUser entity, CancellationToken cancellationToken = default);


        /// <summary>
        /// Delete user
        /// </summary>
        Task<AppUser> DeleteAsync(AppUser entity, CancellationToken cancellationToken = default);
        #endregion
    }
}
