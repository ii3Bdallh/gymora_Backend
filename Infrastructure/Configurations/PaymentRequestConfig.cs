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
  public class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
  {
    public void Configure(EntityTypeBuilder<PaymentRequest> builder)
    {
      // Auditing not enabled for this entity

      builder.ConfigureFileAuditing();

      builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
      builder.Property(x => x.CouponCode).HasMaxLength(50);
      builder.Property(x => x.ReviewNotes).HasMaxLength(1000);
      builder.Property(x => x.RejectionReason).HasMaxLength(1000);

      builder.Property(x => x.OriginalAmount).HasPrecision(18, 2);
      builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
      builder.Property(x => x.FinalAmount).HasPrecision(18, 2);



      builder.HasOne(x => x.ReviewedByUser)
             .WithMany()
             .HasForeignKey(x => x.ReviewedBy)
             .OnDelete(DeleteBehavior.Restrict);

      builder.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
      builder.HasOne(x => x.PlanPrice).WithMany().HasForeignKey(x => x.PlanPriceId).OnDelete(DeleteBehavior.Restrict);
      builder.HasOne(x => x.Coupon).WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Restrict);


      builder.HasIndex(x => new { x.CreatedById, x.Status });

      builder.ToTable(t => t.HasCheckConstraint("CK_PaymentRequests_Amount", "\"FinalAmount\" >= 0"));



    }
  }
}