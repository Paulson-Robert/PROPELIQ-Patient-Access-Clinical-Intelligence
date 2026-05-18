using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class Icd10CodeConfiguration : IEntityTypeConfiguration<Icd10Code>
{
    public void Configure(EntityTypeBuilder<Icd10Code> builder)
    {
        builder.HasKey(c => c.Icd10CodeId);

        builder.Property(c => c.CodeValue)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.CodeSetVersion)
            .IsRequired()
            .HasMaxLength(16);

        // Composite unique constraint — same code may exist across different versions (Edge Case)
        builder.HasIndex(c => new { c.CodeValue, c.CodeSetVersion })
            .IsUnique();

        builder.HasIndex(c => c.CodeSetVersion);
    }
}
