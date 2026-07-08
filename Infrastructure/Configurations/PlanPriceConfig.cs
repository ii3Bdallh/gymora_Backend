using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
  public class PlanPriceConfiguration : IEntityTypeConfiguration<PlanPrice>
  {
    public void Configure(EntityTypeBuilder<PlanPrice> builder)
    {
      // Auditing not enabled for this entity

      builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
      builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
      builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");

      builder.HasOne(x => x.Plan)
             .WithMany(x => x.Prices)
             .HasForeignKey(x => x.PlanId)
             .OnDelete(DeleteBehavior.Restrict);

      builder.HasIndex(x => new { x.PlanId, x.CountryCode, x.CurrencyCode, x.DurationMonths }).IsUnique();

      builder.ToTable(t => t.HasCheckConstraint("CK_PlanPrices_Duration", "\"DurationMonths\" > 0"));
      builder.ToTable(t => t.HasCheckConstraint("CK_PlanPrices_Amount", "\"Amount\" >= 0"));

    }
  }
}
