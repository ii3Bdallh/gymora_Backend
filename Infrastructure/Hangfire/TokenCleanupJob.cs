using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Hangfire
{
    public class TokenCleanupJob(AppDbContext context, ILogger<TokenCleanupJob> logger)
    {
        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var revokedCutoff = DateTime.UtcNow.AddDays(-30); // نسيب شهر buffer قبل الحذف

                var deletedCount = await context.RefreshTokens
                    .Where(rt => rt.ExpirationAt < DateTime.UtcNow || rt.RevokedAt < revokedCutoff)
                    .ExecuteDeleteAsync();

                logger.LogInformation("Hangfire: Cleaned up {Count} expired/revoked refresh tokens", deletedCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hangfire: Error occurred while cleaning up expired tokens");
                throw; // مهم تسيبها throw عشان Hangfire يعتبرها Failed job ويعمل retry
            }
        }
    }
}