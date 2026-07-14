
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service;
using Application.Service.Entity;
using Application.Service.shared;

using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
            // Auth & Core Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();


            // Notification Service
            services.AddScoped<INotificationService, NotificationService>();

            // Trainer Certificate
            services.AddScoped<CurrentUser>();

            // Script will Add After Here DependencyInjectionService
services.AddScoped<ICouponService, CouponService>();
services.AddScoped<IPaymentRequestService, PaymentRequestService>();

        return services;
    }
}