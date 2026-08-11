using Domain.Model;
using Domain.Model.Auth;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class InvitationConfig : IEntityTypeConfiguration<Invitation>
    {
        public void Configure(EntityTypeBuilder<Invitation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Gym)
                   .WithMany()
                   .HasForeignKey(x => x.GymId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CreatedByPerson)
                        .WithMany()
                        .HasForeignKey(e => e.CreatedByPersonId)
                        .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<ApplicationUser>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.GymRole).HasConversion<int>().IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();

            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            builder.Property(x => x.FinalAmount).HasPrecision(18, 2);
            builder.Property(x => x.Salary).HasPrecision(18, 2);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.GymRole);
            builder.HasIndex(x => x.Status);


        }
    }
}
