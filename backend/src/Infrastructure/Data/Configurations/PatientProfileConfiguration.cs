using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// PHI column encryption (FirstName, LastName, DateOfBirth, Phone) is applied
/// in ApplicationDbContext.OnModelCreating after all configurations are applied,
/// using PgCryptoExtensions value converters (AC-02).
/// </summary>
public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> builder)
    {
        builder.HasKey(p => p.PatientProfileId);

        // PHI columns — encryption converters applied in ApplicationDbContext
        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(512); // Max covers base64-encoded ciphertext

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(p => p.DateOfBirth)
            .HasMaxLength(512);

        builder.Property(p => p.Phone)
            .HasMaxLength(512);

        // PHI — encryption converter applied in ApplicationDbContext (AC-02)
        builder.Property(p => p.InsuranceId)
            .HasMaxLength(512); // Max covers base64-encoded ciphertext

        builder.Property(p => p.InsuranceName)
            .HasMaxLength(256);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // One-to-one: PatientProfile → PatientView
        builder.HasOne(p => p.PatientView)
            .WithOne(v => v.PatientProfile)
            .HasForeignKey<PatientView>(v => v.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one: PatientProfile → NoShowRiskFactor
        builder.HasOne(p => p.NoShowRiskFactor)
            .WithOne(r => r.PatientProfile)
            .HasForeignKey<NoShowRiskFactor>(r => r.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: PatientProfile → ClinicalDocuments
        builder.HasMany(p => p.ClinicalDocuments)
            .WithOne(d => d.PatientProfile)
            .HasForeignKey(d => d.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: PatientProfile → IntakeRecords
        builder.HasMany(p => p.IntakeRecords)
            .WithOne(i => i.PatientProfile)
            .HasForeignKey(i => i.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: PatientProfile → ExtractedDataRecords
        builder.HasMany(p => p.ExtractedDataRecords)
            .WithOne(e => e.PatientProfile)
            .HasForeignKey(e => e.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: PatientProfile → DataConflicts
        builder.HasMany(p => p.DataConflicts)
            .WithOne(c => c.PatientProfile)
            .HasForeignKey(c => c.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: PatientProfile → MedicalCodeMappings
        builder.HasMany(p => p.MedicalCodeMappings)
            .WithOne(m => m.PatientProfile)
            .HasForeignKey(m => m.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.CreatedAt);
    }
}
