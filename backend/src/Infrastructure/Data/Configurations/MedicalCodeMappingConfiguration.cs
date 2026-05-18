using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MedicalCodeMappingConfiguration : IEntityTypeConfiguration<MedicalCodeMapping>
{
    public void Configure(EntityTypeBuilder<MedicalCodeMapping> builder)
    {
        builder.HasKey(m => m.MappingId);

        builder.Property(m => m.CodeType)
            .IsRequired();

        builder.Property(m => m.CodeValue)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(m => m.CodeDescription)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(m => m.Confidence)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(m => m.CodeSetVersion)
            .IsRequired()
            .HasMaxLength(32);

        // Many-to-one: MedicalCodeMapping → User (verifier, optional)
        builder.HasOne(m => m.VerifiedByUser)
            .WithMany()
            .HasForeignKey(m => m.VerifiedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.PatientProfileId);
        builder.HasIndex(m => m.ExtractedRecordId);
        builder.HasIndex(m => m.CodeType);
        builder.HasIndex(m => m.IsVerified);
    }
}
