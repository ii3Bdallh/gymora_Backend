using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
    {
        public void Configure(EntityTypeBuilder<MembershipPlan> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Price).HasPrecision(18, 2);

            builder.HasOne(x => x.Gym)
                .WithMany()
                .HasForeignKey(x => x.GymId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByPerson)
                .WithMany()
                .HasForeignKey(x => x.CreatedByPersonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
