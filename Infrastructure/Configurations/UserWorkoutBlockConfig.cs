using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class UserWorkoutBlockConfiguration : IEntityTypeConfiguration<UserWorkoutBlock>
    {
        public void Configure(EntityTypeBuilder<UserWorkoutBlock> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Reason).HasMaxLength(500);

            builder.HasOne(x => x.BlockedUser)
                .WithMany()
                .HasForeignKey(x => x.BlockedUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
