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
       public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
       {
              public void Configure(EntityTypeBuilder<Coupon> builder)
              {
                     // Auditing not enabled for this entity
                     builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
                     builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
                     builder.Property(x => x.Description).HasMaxLength(500);
                     builder.Property(x => x.DiscountValue)
                            .HasPrecision(18, 2);

                     builder.Property(x => x.MaxDiscountAmount)
                            .HasPrecision(18, 2);

                     builder.Property(x => x.MinimumPurchaseAmount)
                            .HasPrecision(18, 2);
                     builder.HasIndex(x => x.Code).IsUnique();


                     builder.ToTable(t => t.HasCheckConstraint("CK_Coupons_DiscountValue", "\"DiscountValue\" > 0"));
              }
       }
}