using Domain.Model.Base;
using Domain.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static void ConfigureAuditing<T>(this EntityTypeBuilder<T> builder)
            where T : BaseAuditableEntity
    {
        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(e => e.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.IsActive);
    }

    public static void ConfigureFileAuditing<T>(this EntityTypeBuilder<T> builder)
        where T : BaseAuditableFileEntity
    {
        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(e => e.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.IsActive);
    }
}
