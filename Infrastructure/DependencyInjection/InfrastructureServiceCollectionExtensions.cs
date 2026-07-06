using Application.Interface;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;

using Infrastructure.Repo;
using Infrastructure.Repo.Entity;
using Infrastructure.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthRepo, AuthRepo>();
        services.AddScoped<IUserRepo, UserRepo>();

        services.AddScoped<ITransactionManager, TransactionManager>();

        services.AddSingleton<ILogger>(sp =>
            sp.GetRequiredService<ILoggerFactory>().CreateLogger("App"));

        // Script will Add After Here DependencyInjectionRepo


        return services;
    }
}