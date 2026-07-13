// Application/Models/CurrentUser.cs
using Domain.Enum;

namespace Application.Model;

/// <summary>
/// Represents the current user in the application.
/// </summary>
public class CurrentUser
{
    public int UserId { get; set; }
    public int? CurrentGymId { get; set; }
    public string? GymRole { get; set; }
    public string? PlatformRole { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool IsSuperAdmin => string.Equals(PlatformRole, AppRole.SuperAdmin, StringComparison.OrdinalIgnoreCase);

    public bool IsInGymRole(string role)
        => string.Equals(GymRole, role, StringComparison.OrdinalIgnoreCase);

    public bool HasGymAccess(int gymId)
        => CurrentGymId == gymId;
}