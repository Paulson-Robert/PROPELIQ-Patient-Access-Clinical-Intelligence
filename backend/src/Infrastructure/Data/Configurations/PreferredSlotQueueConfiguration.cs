using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PreferredSlotQueueConfiguration : IEntityTypeConfiguration<PreferredSlotQueue>
{
    public void Configure(EntityTypeBuilder<PreferredSlotQueue> builder)
    {
        builder.HasKey(q => q.QueueId);

        builder.Property(q => q.Status)
            .IsRequired();

        builder.Property(q => q.RequestedAt)
            .IsRequired();

        // Many-to-one: PreferredSlotQueue → AvailabilitySlot
        builder.HasOne(q => q.PreferredSlot)
            .WithMany(s => s.PreferredSlotQueues)
            .HasForeignKey(q => q.PreferredSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => q.AppointmentId);
        builder.HasIndex(q => q.Status);

        // FCFS ordering: retrieve waiting entries for a slot in request-arrival order (AC-03)
        builder.HasIndex(q => new { q.PreferredSlotId, q.RequestedAt })
            .HasDatabaseName("IX_PreferredSlotQueues_PreferredSlotId_RequestedAt");
    }
}
