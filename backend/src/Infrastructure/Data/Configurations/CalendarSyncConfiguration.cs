using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CalendarSyncConfiguration : IEntityTypeConfiguration<CalendarSync>
{
    public void Configure(EntityTypeBuilder<CalendarSync> builder)
    {
        builder.HasKey(c => c.SyncId);

        builder.Property(c => c.Provider)
            .IsRequired();

        // Tokens are sensitive — stored encrypted (max length covers base64 ciphertext)
        builder.Property(c => c.AccessToken)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(c => c.RefreshToken)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(c => c.TokenExpiry)
            .IsRequired();

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => new { c.UserId, c.Provider });
    }
}
