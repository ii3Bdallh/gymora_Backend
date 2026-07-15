using Domain.Model;
using Domain.Model.Auth;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
  public class OwnerSubscriptionConfiguration : IEntityTypeConfiguration<OwnerSubscription>
  {
    public void Configure(EntityTypeBuilder<OwnerSubscription> builder)
    {
      // Auditing not enabled for this entity
      builder.ConfigureAuditing();


      builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
      builder.Property(x => x.AmountPaid).HasPrecision(18, 2);



      builder.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
      builder.HasOne(x => x.PlanPrice).WithMany().HasForeignKey(x => x.PlanPriceId).OnDelete(DeleteBehavior.Restrict);
      builder.HasOne(x => x.PaymentRequest).WithMany().HasForeignKey(x => x.PaymentRequestId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CreatedById, x.EndDate, x.GraceEndDate });

            builder.ToTable(t => t.HasCheckConstraint("CK_OwnerSubscriptions_Dates", "\"StartDate\" < \"EndDate\" AND \"EndDate\" <= \"GraceEndDate\""));
    }
  }
}