using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// JSONB columns for aggregated clinical data (AC-03).
/// Null JSONB values are valid for patients with no aggregated data yet.
/// </summary>
public class PatientViewConfiguration : IEntityTypeConfiguration<PatientView>
{
    public void Configure(EntityTypeBuilder<PatientView> builder)
    {
        builder.HasKey(v => v.PatientViewId);

        // JSONB columns — accept null without serialization errors (Edge Case)
        builder.Property(v => v.AggregatedVitals)
            .HasColumnType("jsonb");

        builder.Property(v => v.AggregatedMedications)
            .HasColumnType("jsonb");

        builder.Property(v => v.AggregatedAllergies)
            .HasColumnType("jsonb");

        builder.Property(v => v.AggregatedDiagnoses)
            .HasColumnType("jsonb");

        builder.Property(v => v.AggregatedProcedures)
            .HasColumnType("jsonb");

        builder.Property(v => v.VerificationStatus)
            .IsRequired();

        builder.HasIndex(v => v.PatientProfileId).IsUnique();
        builder.HasIndex(v => v.VerificationStatus);
    }
}
