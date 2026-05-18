using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class NoShowRiskFactorConfiguration : IEntityTypeConfiguration<NoShowRiskFactor>
{
    public void Configure(EntityTypeBuilder<NoShowRiskFactor> builder)
    {
        builder.HasKey(r => r.FactorId);

        builder.Property(r => r.AverageLeadTimeDays)
            .IsRequired()
            .HasPrecision(7, 2);

        builder.Property(r => r.PreferredTimeOfDay)
            .HasMaxLength(32);

        builder.Property(r => r.LastCalculatedAt)
            .IsRequired();

        builder.HasIndex(r => r.PatientProfileId).IsUnique();
    }
}
