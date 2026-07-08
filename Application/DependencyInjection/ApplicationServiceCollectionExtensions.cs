using Application.Interface.Service;
using Application.Interface.Service.Entity;
using Application.Interface.Service.Shared;
using Application.Service;
using Application.Service.Entity;
using Application.Service.shared;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IAuthService, AuthService>();
        // Notification
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();





        // Script will Add After Here DependencyInjectionService
services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();

        return services;
    }
}