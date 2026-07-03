using Hangfire;

namespace Infrastructure.Hangfire
{

    public static class RecurringJobs
    {
        public static void Register()
        {
            /*
             * Every minute
             */
            // RecurringJob.AddOrUpdate(
            //     "cleanup-cache",
            //     () => Console.WriteLine("Cleanup cache"),
            //     Cron.Minutely);

            RecurringJob.AddOrUpdate<TokenCleanupJob>(
        "cleanup-expired-tokens",
        job => job.CleanupExpiredTokensAsync(),
        Cron.Daily(3, 0)); // كل يوم الساعة 3 الفجر
        }
    }
}