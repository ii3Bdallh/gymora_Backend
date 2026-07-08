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
  public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
  {
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
      // Auditing not enabled for this entity
      builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
      builder.Property(x => x.Description).HasMaxLength(500);
      builder.HasIndex(x => x.Name).IsUnique();
    }
  }
}
