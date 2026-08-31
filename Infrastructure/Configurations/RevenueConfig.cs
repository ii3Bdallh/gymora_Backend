using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class RevenueConfig : IEntityTypeConfiguration<Revenue>
    {
        public void Configure(EntityTypeBuilder<Revenue> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureGymAuditing();

            builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.PaymentMethod).HasConversion<int>().IsRequired();
            builder.Property(x => x.RevenueCategory).HasConversion<int>().IsRequired();

            builder.HasOne(x => x.GymMember)
                   .WithMany()
                   .HasForeignKey(x => x.GymMemberId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByPerson)
                   .WithMany()
                   .HasForeignKey(x => x.CreatedByPersonId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => new { x.GymId, x.RevenueDate });
            builder.HasIndex(x => new { x.GymId, x.RevenueCategory });
            builder.HasIndex(x => new { x.GymId, x.GymMemberId });
        }
    }
}
