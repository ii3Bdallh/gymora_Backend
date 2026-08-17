using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class MemberWorkoutPlanConfiguration : IEntityTypeConfiguration<MemberWorkoutPlan>
    {
        public void Configure(EntityTypeBuilder<MemberWorkoutPlan> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureGymAuditing(); // Configures Gym and CreatedByPerson relations

            builder.Property(x => x.Goal).HasMaxLength(200);

            // Configure Status to be stored as a string or int? Int is standard for Enums in this codebase.
            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            // Configure relationships with Restrict to avoid multiple cascade paths in SQL Server
            builder.HasOne(x => x.WorkoutPlan)
                .WithMany()
                .HasForeignKey(x => x.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.GymId, x.MemberId, x.Status });
            builder.HasIndex(x => new { x.GymId, x.WorkoutPlanId });
        }
    }
}
