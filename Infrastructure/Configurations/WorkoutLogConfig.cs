using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
    {
        public void Configure(EntityTypeBuilder<WorkoutLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureGymOwned(); // Configures Gym relation and cascade delete

            builder.Property(x => x.WeightUsed).HasPrecision(6, 2);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SessionExercise)
                .WithMany()
                .HasForeignKey(x => x.SessionExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => new { x.GymId, x.MemberId, x.PerformedDate });
            builder.HasIndex(x => new { x.GymId, x.SessionExerciseId, x.PerformedDate });
        }
    }
}
