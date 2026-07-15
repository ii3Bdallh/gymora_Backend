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
  public class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
  {
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
      // Auditing not enabled for this entity
      builder.ConfigureAuditing();

      builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");

      builder.HasOne(x => x.Coupon).WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Restrict);
      builder.HasOne(x => x.PaymentRequest).WithMany().HasForeignKey(x => x.PaymentRequestId).OnDelete(DeleteBehavior.Restrict);



      builder.HasIndex(x => x.CouponId);
    }
  }
}