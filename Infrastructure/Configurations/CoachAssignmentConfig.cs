using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class CoachAssignmentConfiguration : IEntityTypeConfiguration<CoachAssignment>
    {
        public void Configure(EntityTypeBuilder<CoachAssignment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureGymOwned();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Coach)
                .WithMany()
                .HasForeignKey(x => x.CoachStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Gym)
                .WithMany()
                .HasForeignKey(x => x.GymId)
                .OnDelete(DeleteBehavior.Cascade)
                ;


            builder.HasIndex(x => new { x.MemberId, x.CoachStaffId, x.IsActive })
                .IsUnique();
        }
    }
}
