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

    public int? CurrentStaffId { get; set; }
    public string? GymRole { get; set; }
    public string? PlatformRole { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool IsSuperAdmin => string.Equals(PlatformRole, AppRole.SuperAdmin, StringComparison.OrdinalIgnoreCase);
    public bool IsGymOwner => string.Equals(GymRole, GymRoleString.Owner, StringComparison.OrdinalIgnoreCase);
    public bool IsGymManager => string.Equals(GymRole, GymRoleString.Manager, StringComparison.OrdinalIgnoreCase);
    public bool IsGymCoach => string.Equals(GymRole, GymRoleString.Coach, StringComparison.OrdinalIgnoreCase);
    public bool IsGymReceptionist => string.Equals(GymRole, GymRoleString.Receptionist, StringComparison.OrdinalIgnoreCase);
    public bool IsGymCleaner => string.Equals(GymRole, GymRoleString.Cleaner, StringComparison.OrdinalIgnoreCase);
    public bool IsGymOther => string.Equals(GymRole, GymRoleString.Other, StringComparison.OrdinalIgnoreCase);
    public bool IsGymMember => string.Equals(GymRole, GymRoleString.Member, StringComparison.OrdinalIgnoreCase);

}