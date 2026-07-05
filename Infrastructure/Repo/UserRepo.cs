using Application.DTO;
using Application.DTO.Pagintion;
using Infrastructure.Cache;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interface.Repo.Shared;
using Domain.Model.Auth;

namespace Infrastructure.Repo.Entity;

public class UserRepo(ApplicationDbContext context, ILogger logger, UserManager<ApplicationUser> userManager, QueryCache queryCache) : IUserRepo
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly ILogger Logger = logger;
    protected readonly UserManager<ApplicationUser> UserManager = userManager;
    protected readonly QueryCache QueryCache = queryCache;

    public DbSet<ApplicationUser> DbSet => Context.Set<ApplicationUser>();

    #region Read Methods
    public async Task<IEnumerable<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
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

    public IQueryable<ApplicationUser> GetAllQuery(
        PaginatedSearchReq searchReq,
        bool trackChanges = false)
    {
        IQueryable<ApplicationUser> query = DbSet;

        if (!string.IsNullOrEmpty(searchReq.SearchTerm))
            query = query.Where(x =>
                (x.PersonName != null && x.PersonName.Contains(searchReq.SearchTerm)) ||
                (x.Email != null && x.Email.Contains(searchReq.SearchTerm)) ||
                (x.UserName != null && x.UserName.Contains(searchReq.SearchTerm)));

        else
            query = query.OrderByDescending(x => x.Id);

        var result = trackChanges ? query : query.AsNoTracking();
        return result;
    }

    public virtual async Task<PaginatedRes<ApplicationUser>> GetPageAsync(
        PaginatedSearchReq searchReq,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchReq.SearchTerm))
                query = query.Where(x =>
                    (x.PersonName != null && x.PersonName.Contains(searchReq.SearchTerm)) ||
                    (x.Email != null && x.Email.Contains(searchReq.SearchTerm)) ||
                    (x.UserName != null && x.UserName.Contains(searchReq.SearchTerm)));
            else
                query = query.OrderByDescending(x => x.Id);

            if (!trackChanges)
                query = query.AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((searchReq.PageNumber - 1) * searchReq.PageSize)
                .Take(searchReq.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedRes<ApplicationUser>
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

    public async Task<ApplicationUser?> GetByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbSet.Where(x => x.Id == id);
            return trackChanges
                ? await query.FirstOrDefaultAsync(cancellationToken)
                : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user by id");
            throw;
        }
    }

    public async Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var usersInRole = await UserManager.GetUsersInRoleAsync(roleName);
            return usersInRole;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting users by role");
            throw;
        }
    }

    public async Task<PaginatedRes<ApplicationUser>> GetByRolePagedAsync(
        string roleName,
        PaginatedSearchReq searchReq,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var usersInRole = await UserManager.GetUsersInRoleAsync(roleName);
            var query = usersInRole.AsQueryable();

            var totalCount = query.Count();

            var items = query
                .Skip((searchReq.PageNumber - 1) * searchReq.PageSize)
                .Take(searchReq.PageSize)
                .ToList();

            return new PaginatedRes<ApplicationUser>
            {
                PageNumber = searchReq.PageNumber,
                PageSize = searchReq.PageSize,
                TotalCount = totalCount,
                Items = items
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting users by role paged");
            throw;
        }
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await DbSet.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null) return false;
            return await UserManager.IsInRoleAsync(user, roleName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking user role");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await DbSet.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null) return Enumerable.Empty<string>();
            return await UserManager.GetRolesAsync(user);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user roles");
            throw;
        }
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user by email");
            throw;
        }
    }
    #endregion

    #region Write Methods

    public async Task AssignRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await DbSet.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null) throw new Exception("User not found");

            var result = await UserManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning role to user");
            throw;
        }
    }

    public async Task RemoveRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await DbSet.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null) throw new Exception("User not found");

            var result = await UserManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing role from user");
            throw;
        }
    }

    public async Task AssignRolesAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await DbSet.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null) throw new Exception("User not found");

            var result = await UserManager.AddToRolesAsync(user, roleNames);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning roles to user");
            throw;
        }
    }

    public async Task<ApplicationUser> UpdateAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = (ApplicationUser)entity;
            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
            return user;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating user");
            throw;
        }
    }

    public async Task<ApplicationUser> DeleteAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = (ApplicationUser)entity;
            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
            return user;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting user");
            throw;
        }
    }
    #endregion
}
