using Domain.Model.Base;
using Domain.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Model;

namespace Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static void ConfigureAuditing<T>(this EntityTypeBuilder<T> builder)
            where T : BaseAuditableEntity
    {
        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(e => e.CreatedById)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.IsActive);
    }

    public static void ConfigureFileAuditing<T>(this EntityTypeBuilder<T> builder)
        where T : BaseAuditableFileEntity
    {
        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(e => e.CreatedById)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.IsActive);
    }

    public static void ConfigureGymAuditing<T>(this EntityTypeBuilder<T> builder)
    where T : BaseAuditableGymEntity
    {
        builder.HasOne(x => x.CreatedByPerson)
               .WithMany()
               .HasForeignKey(e => e.CreatedByPersonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.IsActive);
    }

    public static void ConfigureGymOwned<T>(this EntityTypeBuilder<T> builder)
        where T : BaseGymEntity
    {
        builder.HasOne(x => x.Gym)
               .WithMany()
               .HasForeignKey(e => e.GymId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.IsActive);
    }
}
