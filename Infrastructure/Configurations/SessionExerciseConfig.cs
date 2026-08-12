using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class SessionExerciseConfiguration : IEntityTypeConfiguration<SessionExercise>
    {
        public void Configure(EntityTypeBuilder<SessionExercise> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExerciseName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(300);
            builder.Property(x => x.WeightKg).HasPrecision(5, 2);

            builder.HasOne(x => x.Session)
                .WithMany(s => s.Exercises)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Exercise)
                .WithMany()
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
