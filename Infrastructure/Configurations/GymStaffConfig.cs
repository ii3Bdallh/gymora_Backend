using Domain.Model;
using Domain.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
  public class GymStaffConfiguration : IEntityTypeConfiguration<GymStaff>
  {
    public void Configure(EntityTypeBuilder<GymStaff> builder)
    {
      // Auditing not enabled for this entity
      builder.ToTable("gym_people");

      builder.Property(x => x.StaffName).HasMaxLength(100).IsRequired();
      builder.Property(x => x.StaffInviteCode).HasMaxLength(100).IsRequired();
      builder.Property(x => x.PhoneNumber).HasMaxLength(50);
      builder.Property(x => x.Email).HasMaxLength(256);
      builder.Property(x => x.Salary).HasPrecision(18, 2);
      builder.Property(x => x.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
      builder.Property(x => x.RowVersion).IsRowVersion();
      builder.Property(x => x.SalaryValidFrom).HasDefaultValueSql("CURRENT_TIMESTAMP");
      builder.Property(x => x.SalaryValidUntil);
      builder.Property(x => x.GymRole).HasConversion<int>();
      

      builder.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);




      builder.HasIndex(x => new { x.GymId, x.UserId, x.GymRole }).IsUnique().HasFilter("\"UserId\" IS NOT NULL");

    }

    
  }
}