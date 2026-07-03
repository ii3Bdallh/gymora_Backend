using Hangfire;
using Hangfire.SqlServer;
using Hangfire.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class HangfireServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(15)
                });
        });

        services.AddHangfireServer();

        return services;
    }
}