using Domain.Interface;

namespace Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(IUser user, IList<string> roles);

    string GenerateRefreshToken();
}
