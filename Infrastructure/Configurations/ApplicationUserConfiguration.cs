using Domain.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.PersonName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);
    }
}
