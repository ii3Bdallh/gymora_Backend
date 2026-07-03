using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class AppUserRoleConfig : IEntityTypeConfiguration<AppUserRole>

    {


        public void Configure(EntityTypeBuilder<AppUserRole> builder)
        {


            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}