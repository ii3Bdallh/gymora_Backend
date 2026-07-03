using System.Security.Claims;
using Application.Common.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.CurrentUser;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return userIdClaim is not null && int.TryParse(userIdClaim, out var id)
                ? id
                : null;
        }
    }

    public string? Email
        => _httpContextAccessor.HttpContext?
            .User?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var roles = _httpContextAccessor.HttpContext?
                .User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return roles?.AsReadOnly() ?? (IReadOnlyCollection<string>)Array.Empty<string>();
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            return _httpContextAccessor.HttpContext?
                .User?.FindFirstValue(ClaimTypes.Role) == RoleConstants.SuperAdmin;
        }
    }

}
