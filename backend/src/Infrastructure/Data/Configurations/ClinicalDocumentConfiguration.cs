using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ClinicalDocumentConfiguration : IEntityTypeConfiguration<ClinicalDocument>
{
    public void Configure(EntityTypeBuilder<ClinicalDocument> builder)
    {
        builder.HasKey(d => d.DocumentId);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(512);

        // Stored as integer — follows existing enum storage pattern (AC-01)
        builder.Property(d => d.FileFormat)
            .IsRequired();

        builder.Property(d => d.StoragePath)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(d => d.MalwareScanStatus)
            .IsRequired();

        builder.Property(d => d.ProcessingStatus)
            .IsRequired();

        builder.Property(d => d.UploadedAt)
            .IsRequired();

        // Stage transition timestamps — nullable until stage is reached (AC-02)
        builder.Property(d => d.ScanningStartedAt);
        builder.Property(d => d.ProcessingStartedAt);

        // One-to-many: ClinicalDocument → ExtractedDataRecords
        builder.HasMany(d => d.ExtractedDataRecords)
            .WithOne(e => e.Document)
            .HasForeignKey(e => e.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.PatientProfileId);
        builder.HasIndex(d => d.ProcessingStatus);
        builder.HasIndex(d => d.UploadedAt);
    }
}
