using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CptCodeConfiguration : IEntityTypeConfiguration<CptCode>
{
    public void Configure(EntityTypeBuilder<CptCode> builder)
    {
        builder.HasKey(c => c.CptCodeId);

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
