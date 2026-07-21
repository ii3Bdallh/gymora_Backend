using Domain.Model;
using Domain.Model.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        int>
{

    // Script will Add After Here DbSet<Entity>
public DbSet<GymStaff> GymStaff { get; set; }
public DbSet<Gym> Gym { get; set; }
public DbSet<CouponRedemption> CouponRedemption { get; set; }
public DbSet<OwnerSubscription> OwnerSubscription { get; set; }
public DbSet<Coupon> Coupon { get; set; }
public DbSet<PaymentRequest> PaymentRequest { get; set; }
public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }
public DbSet<PlanPrice> PlanPrice { get; set; }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }


    public DbSet<Domain.Model.Notification> Notifications
        => Set<Domain.Model.Notification>();

    public DbSet<UserDevice> UserDevices
        => Set<UserDevice>();




    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);


        // 🔥 السطر السحري: MassTransit بتبني هنا جداول الـ Outbox والـ InboxState 
        // لتتبع حالة الـ Consumers ومنع تكرار الـ Notification لو الـ Email فشل
        builder.AddTransactionalOutboxEntities();
    }
}