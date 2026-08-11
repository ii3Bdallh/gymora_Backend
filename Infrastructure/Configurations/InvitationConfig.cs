using Domain.Model;
using Domain.Model.Auth;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class InvitationConfig : IEntityTypeConfiguration<Invitation>
    {
        public void Configure(EntityTypeBuilder<Invitation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ConfigureGymAuditing(); // Automatically configures Gym relation and cascade delete


            builder.HasOne<ApplicationUser>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
