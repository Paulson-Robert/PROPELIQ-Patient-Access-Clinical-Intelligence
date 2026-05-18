using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.AppointmentId);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.BookingType)
            .IsRequired();

        builder.Property(a => a.NoShowRiskScore)
            .HasPrecision(5, 2);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        // Many-to-one: Appointment → AvailabilitySlot (booked slot)
        builder.HasOne(a => a.Slot)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.SlotId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: Appointment → AvailabilitySlot (preferred slot, optional)
        builder.HasOne(a => a.PreferredSlot)
            .WithMany(s => s.PreferredAppointments)
            .HasForeignKey(a => a.PreferredSlotId)
            .OnDelete(DeleteBehavior.SetNull);

        // One-to-many: Appointment → Notifications
        builder.HasMany(a => a.Notifications)
            .WithOne(n => n.Appointment)
            .HasForeignKey(n => n.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: Appointment → PreferredSlotQueues
        builder.HasMany(a => a.PreferredSlotQueues)
            .WithOne(q => q.Appointment)
            .HasForeignKey(q => q.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: Appointment → IntakeRecords
        builder.HasMany(a => a.IntakeRecords)
            .WithOne(i => i.Appointment)
            .HasForeignKey(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.PatientId, a.Status });
    }
}
