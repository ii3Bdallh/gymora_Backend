using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class BodyMeasurementConfiguration : IEntityTypeConfiguration<BodyMeasurement>
    {
        public void Configure(EntityTypeBuilder<BodyMeasurement> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureAuditing(); // Configures CreatedById (ApplicationUser) relation

            builder.HasIndex(x => new { x.CreatedById, x.CreatedOn });

            builder.Property(x => x.WeightKg).HasPrecision(5, 2);
            builder.Property(x => x.HeightCm).HasPrecision(5, 2);
            builder.Property(x => x.BodyFatPercentage).HasPrecision(5, 2);
            builder.Property(x => x.ChestCm).HasPrecision(5, 2);
            builder.Property(x => x.WaistCm).HasPrecision(5, 2);
            builder.Property(x => x.ArmsCm).HasPrecision(5, 2);
            builder.Property(x => x.LegsCm).HasPrecision(5, 2);
            builder.Property(x => x.Notes).HasMaxLength(500);
        }
    }
}
