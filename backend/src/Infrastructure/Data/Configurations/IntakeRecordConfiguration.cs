using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class IntakeRecordConfiguration : IEntityTypeConfiguration<IntakeRecord>
{
    public void Configure(EntityTypeBuilder<IntakeRecord> builder)
    {
        builder.HasKey(i => i.IntakeId);

        builder.Property(i => i.IntakeMode)
            .IsRequired();

        // JSONB columns for structured intake data
        builder.Property(i => i.MedicalHistory)
            .HasColumnType("jsonb");

        builder.Property(i => i.CurrentSymptoms)
            .HasColumnType("jsonb");

        builder.Property(i => i.Medications)
            .HasColumnType("jsonb");

        builder.Property(i => i.Allergies)
            .HasColumnType("jsonb");

        builder.Property(i => i.ReasonForVisit)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(i => i.LastModifiedAt)
            .IsRequired();

        builder.HasIndex(i => i.PatientProfileId);
        builder.HasIndex(i => i.AppointmentId);
    }
}
