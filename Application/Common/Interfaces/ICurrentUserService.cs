namespace Application.Common.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsSuperAdmin { get; }
}
