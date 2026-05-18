using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class InsuranceRecordConfiguration : IEntityTypeConfiguration<InsuranceRecord>
{
    public void Configure(EntityTypeBuilder<InsuranceRecord> builder)
    {
        builder.HasKey(r => r.RecordId);

        builder.Property(r => r.InsuranceName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.InsuranceIdPattern)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(r => r.InsuranceName);
        builder.HasIndex(r => r.IsActive);
    }
}
