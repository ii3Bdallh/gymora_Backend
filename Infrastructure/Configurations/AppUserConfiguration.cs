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
       public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
       {
              public void Configure(EntityTypeBuilder<AppUser> builder)
              {




                     // Relationship Configuration



              }
       }
}
