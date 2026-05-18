using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.HasKey(s => s.SlotId);

        builder.Property(s => s.ProviderName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.Specialty)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(s => s.RecurrencePattern)
            .HasMaxLength(256);

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        // EF Core xmin concurrency token for PostgreSQL (ADD-5)
        builder.UseXminAsConcurrencyToken();

        builder.HasIndex(s => s.ProviderId);
        builder.HasIndex(s => new { s.ProviderId, s.StartTime });
        builder.HasIndex(s => s.IsAvailable);

        // Overlap prevention: a provider cannot have two slots with identical start and end boundaries (AC-01)
        builder.HasIndex(s => new { s.ProviderId, s.StartTime, s.EndTime })
            .IsUnique()
            .HasDatabaseName("IX_AvailabilitySlots_ProviderId_StartTime_EndTime");
    }
}
