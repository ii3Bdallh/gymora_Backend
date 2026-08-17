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

            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Coach)
                .WithMany()
                .HasForeignKey(x => x.CoachStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedBy)
                .WithMany()
                .HasForeignKey(x => x.AssignedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.MemberId, x.CoachStaffId })
                .IsUnique();

            builder.HasIndex(x => new { x.GymId, x.CoachStaffId });
            builder.HasIndex(x => new { x.GymId, x.MemberId });
        }
    }
}
