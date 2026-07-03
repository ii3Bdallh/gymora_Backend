using Infrastructure.Identity;

namespace Infrastructure.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);

    string GenerateRefreshToken();
}
