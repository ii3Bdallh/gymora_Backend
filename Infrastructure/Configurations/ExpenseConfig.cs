using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ExpenseConfig : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureGymAuditing();

            builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.PaymentMethod).HasConversion<int>().IsRequired();
            builder.Property(x => x.ExpenseCategory).HasConversion<int>().IsRequired();

            builder.HasOne(x => x.GymStaff)
                   .WithMany()
                   .HasForeignKey(x => x.GymStaffId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByPerson)
                   .WithMany()
                   .HasForeignKey(e => e.CreatedByPersonId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
