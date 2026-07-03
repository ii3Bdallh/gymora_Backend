using Domain.Model.Base;
using Domain.Enum;
using Domain.Model;
using Domain.Model.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>,
        AppUserRole,
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>,
        IdentityUserToken<int>>
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<AppUserRole> AppUserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Add After Here DbSet<Entity>

        public DbSet<Topic> Topics { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(BaseEntity.IsActive));
                    var trueConstant = Expression.Constant(true);
                    var condition = Expression.Lambda(Expression.Equal(property, trueConstant), parameter);
                    builder.Entity(entityType.ClrType).HasQueryFilter(condition);
                }
            }

            builder.Entity<AppRole>().HasData(
                new AppRole { Id = 1, Name = "Owner", NormalizedName = "OWNER" },
                new AppRole { Id = 2, Name = "Admin", NormalizedName = "ADMIN" },
                new AppRole { Id = 3, Name = "User", NormalizedName = "USER" },
                new AppRole { Id = 4, Name = "Guest", NormalizedName = "GUEST" },
                new AppRole { Id = 5, Name = "Teacher", NormalizedName = "TEACHER" },
                new AppRole { Id = 6, Name = "Parent", NormalizedName = "PARENT" }
            );







        }



    }
}