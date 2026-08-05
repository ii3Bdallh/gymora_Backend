using Application.Interface;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Domain.Model;

using Infrastructure.Repo;
using Infrastructure.Repo.Base;
using Infrastructure.Repo.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DependencyInjection;

public static class RepositoryCollectionExtensions
{
    public static IServiceCollection AddInfrastructureRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthRepo, AuthRepo>();
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<IGymAccessRepo, GymAccessRepo>();


        services.AddSingleton<ILogger>(sp =>
            sp.GetRequiredService<ILoggerFactory>().CreateLogger("App"));

        // Script will Add After Here DependencyInjectionRepo
        services.AddScoped<IGymPersonRepo, GymPersonRepo>();
        services.AddScoped<IUsersRepo, UsersRepo>();
        services.AddScoped<IGymRepo, GymRepo>();
        services.AddScoped<ICouponRedemptionRepo, CouponRedemptionRepo>();
        services.AddScoped<IOwnerSubscriptionRepo, OwnerSubscriptionRepo>();
        services.AddScoped<ICouponRepo, CouponRepo>();
        services.AddScoped<IPaymentRequestRepo, PaymentRequestRepo>();
        services.AddScoped<ISubscriptionPlanRepo, SubscriptionPlanRepo>();
        services.AddScoped<ICoachAssignmentRepo, CoachAssignmentRepo>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();


        return services;
    }
}