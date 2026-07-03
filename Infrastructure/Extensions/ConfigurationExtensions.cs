using Domain.Model.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureAuditing<T>(this EntityTypeBuilder<T> builder)
                where T : AuditableEntity
        {
            builder.HasOne(e => e.CreatedBy)
                   .WithMany()
                   .HasForeignKey(e => e.CreatedById)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.IsActive);

        }
    }
}
