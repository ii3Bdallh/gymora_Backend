using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Interface;

namespace Application.Interface.Repo.Entity;

public interface IUserRepo
{
    Task<IEnumerable<IUser>> GetAllAsync(CancellationToken cancellationToken = default);

    IQueryable<IUser> GetAllQuery(PaginatedSearchReq searchReq, bool trackChanges = false);

    Task<PaginatedRes<IUser>> GetPageAsync(
        PaginatedSearchReq searchReq,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task<IUser?> GetByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default);

    Task<IEnumerable<IUser>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default);

    Task<PaginatedRes<IUser>> GetByRolePagedAsync(
        string roleName,
        PaginatedSearchReq searchReq,
        CancellationToken cancellationToken = default);

    Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

    Task<IUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AssignRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

    Task RemoveRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

    Task AssignRolesAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

    Task<IUser> UpdateAsync(IUser entity, CancellationToken cancellationToken = default);

    Task<IUser> DeleteAsync(IUser entity, CancellationToken cancellationToken = default);
}
