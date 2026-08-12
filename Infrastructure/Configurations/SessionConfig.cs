using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureAuditing(); // Configures CreatedById relation

            builder.Property(x => x.SessionName).HasMaxLength(100).IsRequired();

            // Cascade delete exercises in session
            builder.HasMany(x => x.Exercises)
                .WithOne(e => e.Session)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            // Link to WorkoutPlan (One-to-Many)
            builder.HasOne(x => x.WorkoutPlan)
                .WithMany(p => p.Sessions)
                .HasForeignKey(x => x.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
