
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
        services.AddScoped<ICurrentPlanService, CurrentPlanService>();
        services.AddScoped<IGymPersonService, GymPersonService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGymService, GymService>();
        services.AddScoped<ICouponRedemptionService, CouponRedemptionService>();
        services.AddScoped<IOwnerSubscriptionService, OwnerSubscriptionService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IPaymentRequestService, PaymentRequestService>();
        services.AddScoped<ICoachAssignmentService, CoachAssignmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IMembershipPlanService, MembershipPlanService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IRevenueService, RevenueService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IFinancialReportService, FinancialReportService>();

        return services;
    }
}