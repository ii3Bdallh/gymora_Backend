using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.DeviceToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.DeviceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AppVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastUsedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.DeviceToken).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
