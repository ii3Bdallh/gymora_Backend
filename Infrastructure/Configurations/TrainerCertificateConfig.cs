using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class TrainerCertificateConfiguration : IEntityTypeConfiguration<TrainerCertificate>
{
    public void Configure(EntityTypeBuilder<TrainerCertificate> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TrainerId).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(200).IsRequired();
    }
}
