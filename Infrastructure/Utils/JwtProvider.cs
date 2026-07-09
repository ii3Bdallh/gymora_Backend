// Infrastructure/Utils/JwtProvider.cs
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Options;
using Domain.Model.Auth;
using Microsoft.Extensions.Options;

namespace Infrastructure.Utils
{
    public class JwtProvider
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtProvider> _logger;

        public JwtProvider(
            JwtOptions jwtOptions,
            IConfiguration configuration,
            ILogger<JwtProvider> logger)
        {
            _jwtOptions = jwtOptions;
            _configuration = configuration;
            _logger = logger;
        }

        public (string token, int expireInMinutes) GenerateToken(
            ApplicationUser appUser,
            IList<string> roles,
            int? currentGymId = null,
            string? gymRole = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                new Claim(ClaimTypes.Email, appUser.Email ?? string.Empty),
                new Claim("UserId", appUser.Id.ToString()),
            };

            // App Roles (SuperAdmin, PlatformAdmin, ...)
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Gym Specific Claims
            if (currentGymId.HasValue)
            {
                claims.Add(new Claim("CurrentGymId", currentGymId.Value.ToString()));
            }

            if (!string.IsNullOrEmpty(gymRole))
            {
                claims.Add(new Claim("GymRole", gymRole));
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey!));
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return (
                token: new JwtSecurityTokenHandler().WriteToken(token),
                expireInMinutes: _jwtOptions.AccessTokenExpirationMinutes
            );
        }

        // Generate Token after Switch Gym
        public (string token, int expireInMinutes) GenerateTokenWithGym(
            ApplicationUser appUser,
            IList<string> roles,
            int currentGymId,
            string gymRole)
        {
            return GenerateToken(appUser, roles, currentGymId, gymRole);
        }

        public string? GetUserIdByToken(string token, bool validateLifetime = true)
        {
            var handler = new JwtSecurityTokenHandler();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey!));

            try
            {
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = validateLifetime,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmetricSecurityKey
                }, out SecurityToken validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;
                return jwtToken?.Subject;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string googleIdToken)
        {
            string? googleClientId = _configuration["Authentication:Google:FlutterClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                _logger.LogError("Google Client Id is not configured");
                throw new Exception("Google Client Id is not configured");
            }

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };

                return await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid Google token");
                return null;
            }
        }
    }
}