using Domain.Model;
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
  public class GymConfiguration : IEntityTypeConfiguration<Gym>
  {
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
      // Auditing not enabled for this entity

      builder.HasIndex(g => g.IsActive);



      // ✅ تعيين أطوال وقيود الأعمدة
      builder.Property(g => g.Name).HasMaxLength(200).IsRequired();


      builder.Property(g => g.Latitude).HasPrecision(10, 7);
      builder.Property(g => g.Longitude).HasPrecision(10, 7);

      // ✅ تحويل Enum إلى int (تخزين كأرقام)
      builder.Property(g => g.Status).HasConversion<int>();



      // ✅ ROWVERSION (التزامن)
      builder.Property(g => g.RowVersion).IsRowVersion();




      // ✅ الفهارس
      builder.HasIndex(g => g.Status);


      // ✅ تطبيق إعدادات التدقيق (Auditing)

    }
  }
}