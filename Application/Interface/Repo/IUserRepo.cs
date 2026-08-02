using Application.DTO;
using Application.DTO.Pagintion;
using Domain.Model.Auth;


namespace Application.Interface.Repo.Shared;

public interface IUserRepo
{
    Task<IEnumerable<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default);

    IQueryable<ApplicationUser> GetAllQuery(PaginatedSearchReq searchReq, bool trackChanges = false);

    Task<PaginatedRes<ApplicationUser>> GetPageAsync(
        PaginatedSearchReq searchReq,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default);

    Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default);

    Task<PaginatedRes<ApplicationUser>> GetByRolePagedAsync(
        string roleName,
        PaginatedSearchReq searchReq,
        CancellationToken cancellationToken = default);

    Task<bool> UserHasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AssignRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

    Task RemoveRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

    Task AssignRolesAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

    Task<ApplicationUser> UpdateAsync(ApplicationUser entity, CancellationToken cancellationToken = default);

    Task<ApplicationUser> DeleteAsync(ApplicationUser entity, CancellationToken cancellationToken = default);

    Task<bool> IsPhoneNumberUsedByOtherUserAsync(string phoneNumber, int currentUserId, CancellationToken cancellationToken = default);
}
