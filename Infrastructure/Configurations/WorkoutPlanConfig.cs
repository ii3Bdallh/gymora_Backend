using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
    {
        public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureFileAuditing(); // Configures CreatedById (ApplicationUser) relation

            builder.Property(x => x.PlanName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            // Cascade delete sessions when plan is deleted
            builder.HasMany(x => x.Sessions)
                .WithOne(s => s.WorkoutPlan)
                .HasForeignKey(s => s.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
