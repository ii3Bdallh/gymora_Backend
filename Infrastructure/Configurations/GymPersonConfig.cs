using Domain.Model;
using Domain.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class GymPersonConfiguration : IEntityTypeConfiguration<GymPerson>
    {
        public void Configure(EntityTypeBuilder<GymPerson> builder)
        {

            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(256);
            builder.Property(x => x.Gender).HasMaxLength(50);
            builder.Property(x => x.PhotoUrl).HasMaxLength(500);
            builder.Property(x => x.InviteCode).IsRequired();
            builder.Property(x => x.CreatedOn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.PersonType).HasConversion<int>().IsRequired();
            builder.Property(x => x.RowVersion).IsRowVersion();

            // Link to AspNetUsers
            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Audit Creator User Link
            builder.HasOne<ApplicationUser>()
                   .WithMany()
                   .HasForeignKey(x => x.CreatedById)
                   .OnDelete(DeleteBehavior.Restrict);

            // Link to Gym (standard BaseGymEntity config)
            builder.HasOne(x => x.Gym)
                   .WithMany()
                   .HasForeignKey(x => x.GymId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.GymId, x.PersonType });


            // Unique Constraints
            builder.HasIndex(x => new { x.GymId, x.PhoneNumber }).IsUnique();
            builder.HasIndex(x => new { x.GymId, x.UserId }).IsUnique().HasFilter("\"UserId\" IS NOT NULL");
            builder.HasIndex(x => new { x.GymId, x.InviteCode }).IsUnique();
        }
    }

    public class GymStaffProfileConfiguration : IEntityTypeConfiguration<GymStaffProfile>
    {
        public void Configure(EntityTypeBuilder<GymStaffProfile> builder)
        {

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("GymPersonId").ValueGeneratedNever();

            builder.Property(x => x.GymRoleId).HasConversion<int>().IsRequired();
            builder.Property(x => x.Salary).HasPrecision(18, 2);

            builder.HasOne(x => x.GymPerson)
                   .WithOne(x => x.StaffProfile)
                   .HasForeignKey<GymStaffProfile>(x => x.Id)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class GymMemberProfileConfiguration : IEntityTypeConfiguration<GymMemberProfile>
    {
        public void Configure(EntityTypeBuilder<GymMemberProfile> builder)
        {

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("GymPersonId").ValueGeneratedNever();

            builder.Property(x => x.MedicalNotes).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasOne(x => x.GymPerson)
                   .WithOne(x => x.MemberProfile)
                   .HasForeignKey<GymMemberProfile>(x => x.Id)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
