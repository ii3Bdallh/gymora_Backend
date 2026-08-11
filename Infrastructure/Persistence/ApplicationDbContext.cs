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
    public DbSet<GymPerson> GymPerson { get; set; }
    public DbSet<GymStaffProfile> GymStaffProfile { get; set; }
    public DbSet<GymMemberProfile> GymMemberProfile { get; set; }
    public DbSet<Gym> Gym { get; set; }
    public DbSet<CouponRedemption> CouponRedemption { get; set; }
    public DbSet<OwnerSubscription> OwnerSubscription { get; set; }
    public DbSet<Coupon> Coupon { get; set; }
    public DbSet<PaymentRequest> PaymentRequest { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }
    public DbSet<PlanPrice> PlanPrice { get; set; }
    public DbSet<CoachAssignment> CoachAssignment { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<MembershipPlan> MembershipPlans { get; set; }
    public DbSet<Invitation> Invitation { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }


    public DbSet<Domain.Model.Notification> Notifications { get; set; }


    public DbSet<UserDevice> UserDevices { get; set; }





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