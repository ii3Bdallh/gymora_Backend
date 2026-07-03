using Domain.Model.Base;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static void ConfigureAuditing<T>(this EntityTypeBuilder<T> builder)
            where T : AuditableEntity
    {
        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(e => e.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.IsActive);
    }
}
