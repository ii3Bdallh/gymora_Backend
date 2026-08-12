using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureFileAuditing(); // Configures CreatedById (ApplicationUser) relation

            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.VideoUrl).HasMaxLength(1000);

            builder.Property(x => x.PrimaryMuscle)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.SecondaryMuscle)
                .HasConversion<int>();

            builder.Property(x => x.Equipment)
                .HasConversion<int>();
        }
    }
}
