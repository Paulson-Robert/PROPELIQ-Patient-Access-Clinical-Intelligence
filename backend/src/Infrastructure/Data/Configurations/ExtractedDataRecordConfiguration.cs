using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ExtractedDataRecordConfiguration : IEntityTypeConfiguration<ExtractedDataRecord>
{
    public void Configure(EntityTypeBuilder<ExtractedDataRecord> builder)
    {
        builder.HasKey(e => e.RecordId);

        builder.Property(e => e.DataType)
            .IsRequired();

        builder.Property(e => e.FieldName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.FieldValue)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(e => e.Confidence)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(e => e.SourceLocation)
            .HasMaxLength(512);

        // Many-to-one: ExtractedDataRecord → User (verifier, optional)
        builder.HasOne(e => e.VerifiedByUser)
            .WithMany()
            .HasForeignKey(e => e.VerifiedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // One-to-many: ExtractedDataRecord → MedicalCodeMappings
        builder.HasMany(e => e.MedicalCodeMappings)
            .WithOne(m => m.ExtractedRecord)
            .HasForeignKey(m => m.ExtractedRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.PatientProfileId);
        builder.HasIndex(e => e.DocumentId);
        builder.HasIndex(e => e.DataType);
        builder.HasIndex(e => e.IsVerified);
    }
}
