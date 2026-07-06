using Hangfire;

namespace Infrastructure.Hangfire
{

    public static class RecurringJobs
    {
        public static void Register()
        {
       
            RecurringJob.AddOrUpdate<TokenCleanupJob>(
                "cleanup-expired-tokens",
                job => job.CleanupExpiredTokensAsync(),
                Cron.Daily(3, 0)); // Daily at 3:00 AM
        }
    }
}