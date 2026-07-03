using Application.DTO;
using Application.DTO.Pagintion;
using Application.Interface.Repo.Entity;
using Domain.Model;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Cache;

namespace Infrastructure.Repo.Entity
{
    /// <summary>
    /// User repository implementation for managing AppUser entities and role assignments.
    /// Provides user management capabilities including role assignment and role-based queries.
    /// </summary>
    public class UserRepo(AppDbContext context, ILogger logger, UserManager<AppUser> userManager, QueryCache queryCache) : IUserRepo
    {
        protected readonly AppDbContext Context = context;
        protected readonly ILogger Logger = logger;
        protected readonly UserManager<AppUser> UserManager = userManager;
        protected readonly QueryCache QueryCache = queryCache;

        public DbSet<AppUser> DbSet => Context.Set<AppUser>();

        #region Read Methods
        public async Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public virtual IQueryable<AppUser> GetAllQuery(
            PaginatedSearchReq searchReq,
            bool trackChanges = false)
        {
            IQueryable<AppUser> query = DbSet;

            if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                query = query.Where(x =>
                    (x.PersonName != null && x.PersonName.Contains(searchReq.SearchTerm)) ||
                    (x.Email != null && x.Email.Contains(searchReq.SearchTerm)) ||
                    (x.UserName != null && x.UserName.Contains(searchReq.SearchTerm)));

            else
                query = query.OrderByDescending(x => x.Id);

            return trackChanges ? query : query.AsNoTracking();
        }

        public virtual async Task<PaginatedRes<AppUser>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = GetAllQuery(searchReq, trackChanges);
                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .Skip((searchReq.PageNumber - 1) * searchReq.PageSize)
                    .Take(searchReq.PageSize)
                    .ToListAsync(cancellationToken);

                return new PaginatedRes<AppUser>
                {
                    PageNumber = searchReq.PageNumber,
                    PageSize = searchReq.PageSize,
                    TotalCount = totalCount,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting paginated users");
                throw;
            }
        }

        public async Task<AppUser?> GetByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = DbSet.Where(x => x.Id == id);
                return trackChanges ?
                    await query.FirstOrDefaultAsync(cancellationToken) :
                    await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting user by ID {Id}", id);
                throw;
            }
        }

        public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await DbSet
                    .Where(x => x.Email == email)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting user by email");
                throw;
            }
        }

        public async Task<IEnumerable<AppUser>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                var usersInRole = await UserManager.GetUsersInRoleAsync(roleName);
                return usersInRole;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting users by role {RoleName}", roleName);
                throw;
            }
        }

        public async Task<PaginatedRes<AppUser>> GetByRolePagedAsync(
            string roleName,
            PaginatedSearchReq searchReq,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var allUsersInRole = await UserManager.GetUsersInRoleAsync(roleName);
                var query = allUsersInRole.AsQueryable();

                if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                    query = query.Where(x =>
                        x.PersonName.Contains(searchReq.SearchTerm) ||
                        (x.Email != null && x.Email.Contains(searchReq.SearchTerm)));

                var totalCount = query.Count();
                var items = query
                    .Skip((searchReq.PageNumber - 1) * searchReq.PageSize)
                    .Take(searchReq.PageSize)
                    .ToList();

                return new PaginatedRes<AppUser>
                {
                    PageNumber = searchReq.PageNumber,
                    PageSize = searchReq.PageSize,
                    TotalCount = totalCount,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting paginated users by role {RoleName}", roleName);
                throw;
            }
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return false;

                return await UserManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error checking user role for user {UserId} and role {RoleName}", userId, roleName);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return Enumerable.Empty<string>();

                return await UserManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting user roles for user {UserId}", userId);
                throw;
            }
        }
        #endregion

        #region Write Methods
        public async Task AssignRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    throw new InvalidOperationException($"User with ID {userId} not found");

                var hasRole = await UserManager.IsInRoleAsync(user, roleName);
                if (!hasRole)
                {
                    var result = await UserManager.AddToRoleAsync(user, roleName);
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"Failed to assign role {roleName} to user {userId}");
                }

                Logger.LogInformation("Role {RoleName} assigned to user {UserId}", roleName, userId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
                throw;
            }
        }

        public async Task AssignRolesAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    throw new InvalidOperationException($"User with ID {userId} not found");

                var userRoles = await UserManager.GetRolesAsync(user);
                var rolesToAdd = roleNames.Except(userRoles).ToList();

                if (rolesToAdd.Any())
                {
                    var result = await UserManager.AddToRolesAsync(user, rolesToAdd);
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"Failed to assign roles to user {userId}");
                }

                Logger.LogInformation("Roles assigned to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error assigning roles to user {UserId}", userId);
                throw;
            }
        }

        public async Task RemoveRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    throw new InvalidOperationException($"User with ID {userId} not found");

                var hasRole = await UserManager.IsInRoleAsync(user, roleName);
                if (hasRole)
                {
                    var result = await UserManager.RemoveFromRoleAsync(user, roleName);
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"Failed to remove role {roleName} from user {userId}");
                }

                Logger.LogInformation("Role {RoleName} removed from user {UserId}", roleName, userId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
                throw;
            }
        }

        public async Task<AppUser> UpdateAsync(AppUser entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await UserManager.UpdateAsync(entity);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to update user {entity.Id}");

                await Context.SaveChangesAsync(cancellationToken);
                return entity;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating user {UserId}", entity.Id);
                throw;
            }
        }

        public async Task<AppUser> DeleteAsync(AppUser entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await UserManager.DeleteAsync(entity);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to delete user {entity.Id}");

                await Context.SaveChangesAsync(cancellationToken);
                return entity;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting user {UserId}", entity.Id);
                throw;
            }
        }


        #endregion
    }
}
