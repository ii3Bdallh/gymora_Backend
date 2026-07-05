using Domain.Model;
using Domain.Model.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        int>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<OutboxMessage> OutboxMessages
        => Set<OutboxMessage>();

    public DbSet<Domain.Model.Notification> Notifications
        => Set<Domain.Model.Notification>();

    public DbSet<UserDevice> UserDevices
        => Set<UserDevice>();

    public DbSet<TestEntity> Tests
        => Set<TestEntity>();

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
    }
}
