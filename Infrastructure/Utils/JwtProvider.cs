using Domain.Model.Json;
using Infrastructure.Identity;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Utils
{
    public class JwtProvider
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtProvider> _logger;

        public JwtProvider(JwtOptions jwtOptions, IConfiguration configuration, ILogger<JwtProvider> logger)
        {
            _jwtOptions = jwtOptions;
            _configuration = configuration;
            _logger = logger;
        }

        public string? GetUserIdByToken(string token, bool validateLifetime = true)
        {
            var handler = new JwtSecurityTokenHandler();
            var symmeticeSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey!));
            try
            {
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.audience,
                    ValidateLifetime = validateLifetime,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmeticeSecurityKey
                }, out SecurityToken validatedToken);

                var jwtSecurityToken = validatedToken as JwtSecurityToken;
                if (jwtSecurityToken == null) return null;
                return jwtSecurityToken.Subject;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token expired");
                return null;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error validating token");
                return null;
            }
        }

        public (string token, int expireInMinutes) GenerateToken(ApplicationUser appUser, IList<string> roles)
        {
            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Sub, appUser.Id.ToString()),
                new(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, appUser.Email!),
                new(JwtRegisteredClaimNames.Name, appUser.PersonName!),
            ];
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var symmeticeSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey!));

            var credentials = new SigningCredentials(symmeticeSecurityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.issuer,
                audience: _jwtOptions.audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.TokenExpirationInMinutes),
                signingCredentials: credentials
            );

            return (
                token: new JwtSecurityTokenHandler().WriteToken(token),
                expireInMinutes: _jwtOptions.TokenExpirationInMinutes
            );
        }

        public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string googleIdToken)
        {
            string? googleClientId = _configuration["Authentication:Google:FlutterClientId"];
            if (googleClientId is null)
            {
                _logger.LogError("Google Client Id is not configured");
                throw new Exception("Google Client Id is not configured");
            }
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
                return payload;
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google token verification failed");
                return null;
            }
        }
    }
}
