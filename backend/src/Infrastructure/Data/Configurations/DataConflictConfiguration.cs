using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// DataConflict tracks conflicting clinical data across two source documents (AC-05).
/// Dual FK references to ClinicalDocument require explicit relationship configuration.
/// </summary>
public class DataConflictConfiguration : IEntityTypeConfiguration<DataConflict>
{
    public void Configure(EntityTypeBuilder<DataConflict> builder)
    {
        builder.HasKey(c => c.ConflictId);

        builder.Property(c => c.ConflictType)
            .IsRequired();

        builder.Property(c => c.FieldName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.Value1)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(c => c.Value2)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(c => c.ResolutionStatus)
            .IsRequired();

        builder.Property(c => c.ResolutionNotes)
            .HasMaxLength(2048);

        // Many-to-one: DataConflict → ClinicalDocument (source document 1)
        builder.HasOne(c => c.SourceDocument1)
            .WithMany()
            .HasForeignKey(c => c.SourceDocumentId1)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: DataConflict → ClinicalDocument (source document 2)
        builder.HasOne(c => c.SourceDocument2)
            .WithMany()
            .HasForeignKey(c => c.SourceDocumentId2)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: DataConflict → User (resolver, optional)
        builder.HasOne(c => c.ResolvedByUser)
            .WithMany()
            .HasForeignKey(c => c.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.PatientProfileId);
        builder.HasIndex(c => c.ResolutionStatus);
        builder.HasIndex(c => new { c.PatientProfileId, c.ResolutionStatus });
    }
}
